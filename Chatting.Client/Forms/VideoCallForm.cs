using Chatting.Client.Services;
using Chatting.Shared.DTOs;
using Chatting.Shared.Enums;
using Microsoft.Web.WebView2.Core;
using System.Drawing.Imaging;
using System.Text.Json;

namespace Chatting.Client.Forms;

public partial class VideoCallForm : Form
{
    private readonly CallInfoDto _callInfo;
    private readonly bool _isInitiator;
    private readonly SignalRService _signalR;
    private readonly SessionManager _session;
    private readonly ToolTip _controlToolTip = new();

    private int _secondsElapsed = 0;
    private bool _isMicOn = true;
    private bool _isCamOn = true;
    private bool _isWebViewReady = false;
    private bool _isMediaReady = false;
    private bool _isInitSent = false;
    private bool _isCallAccepted = false;
    private bool _isConnected = false;
    private bool _isCallEnded = false;
    private bool _localPeerReady = false;
    private bool _remotePeerReady = false;
    private bool _offerStarted = false;
    private string? _remoteAvatarDataUrl;

    // SignalR can deliver SDP/ICE before the WebView has created its peer connection.
    // Keep every signal in arrival order until JavaScript has finished getUserMedia.
    private readonly Queue<WebRtcSignalDto> _pendingSignals = new();
    private System.Windows.Forms.Timer? _timeoutTimer;

    public VideoCallForm(CallInfoDto callInfo, bool isInitiator, SignalRService signalR, SessionManager session)
    {
        _callInfo = callInfo;
        _isInitiator = isInitiator;
        _signalR = signalR;
        _session = session;
        InitializeComponent();

        btnToggleMic.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        btnToggleCam.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        btnEndCall.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        btnEndCall.Text = "  Kết thúc";
        IconHelper.SetButtonIcon(btnEndCall, IconHelper.PhoneOff, 14, Color.White, ContentAlignment.MiddleLeft);
        btnEndCall.BackColor = Color.FromArgb(239, 68, 68);
        btnEndCall.AccessibleName = "Kết thúc cuộc gọi";
        btnEndCall.AccessibleDescription = "Kết thúc cuộc gọi hiện tại.";
        _controlToolTip.ShowAlways = true;
        _controlToolTip.SetToolTip(btnEndCall, "Kết thúc cuộc gọi");
        UpdateMicrophoneButton();
        UpdateCameraButton();
        FormClosed += (_, _) => _controlToolTip.Dispose();
    }

    private async void VideoCallForm_Load(object? sender, EventArgs e)
    {
        string remoteUser = _isInitiator
            ? (!string.IsNullOrWhiteSpace(_callInfo.ReceiverUsername) ? _callInfo.ReceiverUsername : "người nhận")
            : _callInfo.CallerUsername;

        Text = _callInfo.CallType == CallType.Video
            ? $"Cuộc Gọi Video - Chatting ({remoteUser})"
            : $"Cuộc Gọi Thoại - Chatting ({remoteUser})";

        btnToggleCam.Visible = _callInfo.CallType == CallType.Video;
        _ = LoadRemoteAvatarAsync(_isInitiator ? _callInfo.ReceiverAvatarUrl : _callInfo.CallerAvatarUrl);

        if (_isInitiator)
        {
            lblCallInfo.Text = $"Đang gọi cho {remoteUser}...";
            lblCallInfo.ForeColor = Color.FromArgb(56, 189, 248);

            // Bắt đầu chuông chờ gọi đi
            SoundNotificationService.Instance.StartCallingTone();

            // Khởi tạo timeout 45 giây nếu người nhận không trả lời
            _timeoutTimer = new System.Windows.Forms.Timer { Interval = 45000 };
            _timeoutTimer.Tick += HandleCallTimeout;
            _timeoutTimer.Start();
        }
        else
        {
            lblCallInfo.Text = $"Đang kết nối cuộc gọi với {_callInfo.CallerUsername}...";
            lblCallInfo.ForeColor = Color.FromArgb(234, 179, 8);
        }

        // Đăng ký nhận sự kiện SignalR
        _signalR.OnReceiveWebRtcSignal += HandleIncomingSignal;
        _signalR.OnCallAccepted += HandleCallAccepted;
        _signalR.OnCallEnded += HandleCallEnded;

        await InitializeWebViewAsync();
    }

    private async Task LoadRemoteAvatarAsync(string? avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(avatarUrl)) return;
        try
        {
            Image? image = await AvatarImageCache.GetAsync(_session.ServerUrl, avatarUrl);
            if (image is null || IsDisposed) return;

            // Pass a small local image to WebView2. Its HTTPS page cannot reliably
            // display an avatar hosted by the LAN server over HTTP.
            using Bitmap avatar = AvatarImageCache.CreateCircularFit(
                image, new Size(160, 160), Color.FromArgb(30, 41, 59), Color.FromArgb(148, 163, 184));
            using MemoryStream stream = new();
            avatar.Save(stream, ImageFormat.Png);
            _remoteAvatarDataUrl = "data:image/png;base64," + Convert.ToBase64String(stream.ToArray());
            SendToJs(new { action = "remoteAvatar", dataUrl = _remoteAvatarDataUrl });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[VideoCallForm] Avatar error: {ex.Message}");
        }
    }

    private async void HandleCallTimeout(object? sender, EventArgs e)
    {
        StopTimeoutTimer();
        SoundNotificationService.Instance.StopCallingTone();

        if (!_isCallAccepted && !_isConnected && !_isCallEnded)
        {
            _isCallEnded = true;
            lblCallInfo.Text = "Người nhận không trả lời";
            lblCallInfo.ForeColor = Color.FromArgb(239, 68, 68);
            try
            {
                await _signalR.EndCallAsync(_callInfo.CallId);
            }
            catch { }

            MessageBox.Show("Người nhận không trả lời cuộc gọi.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
    }

    private void StopTimeoutTimer()
    {
        if (_timeoutTimer != null)
        {
            _timeoutTimer.Stop();
            _timeoutTimer.Tick -= HandleCallTimeout;
            _timeoutTimer.Dispose();
            _timeoutTimer = null;
        }
    }

    private async Task InitializeWebViewAsync()
    {
        try
        {
            string assetsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
            string htmlPath = Path.Combine(assetsPath, "webrtc_call.html");
            if (!File.Exists(htmlPath))
            {
                MessageBox.Show("Không tìm thấy file Assets/webrtc_call.html!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // WebRTC LAN: Bỏ tính năng giấu IP nội bộ mDNS để 2 máy Windows trong mạng LAN tìm thấy IP trực tiếp
            string insecureServerOrigin = string.Empty;
            if (Uri.TryCreate(_session.ServerUrl, UriKind.Absolute, out Uri? serverUri) &&
                serverUri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
            {
                // The API is normally an HTTP LAN address. Mark only that exact origin as
                // secure; the WebRTC page itself is served through the HTTPS virtual host.
                insecureServerOrigin = " --unsafely-treat-insecure-origin-as-secure=" +
                    serverUri.GetLeftPart(UriPartial.Authority);
            }

            var envOptions = new CoreWebView2EnvironmentOptions(
                "--disable-features=WebRtcHideLocalIpsWithMdns " +
                "--force-webrtc-ip-handling-policy=default_public_and_private_interfaces " +
                "--autoplay-policy=no-user-gesture-required " +
                "--use-fake-ui-for-media-stream " +
                "--allow-file-access-from-files" + insecureServerOrigin);

            // Tách thư mục User Data của WebView2 theo từng tài khoản để khi test 2 client trên cùng 1 máy không bị khóa profile
            string userFolder = Path.Combine(Path.GetTempPath(), "Chatting_WebView2_" + (_session.CurrentUser?.Id.ToString() ?? Guid.NewGuid().ToString()));
            var env = await CoreWebView2Environment.CreateAsync(null, userFolder, envOptions);
            await wvVideoCall.EnsureCoreWebView2Async(env);

            // Tự động cấp quyền Camera và Microphone cho WebView2
            wvVideoCall.CoreWebView2.PermissionRequested += (s, args) =>
            {
                if (args.PermissionKind == CoreWebView2PermissionKind.Camera ||
                    args.PermissionKind == CoreWebView2PermissionKind.Microphone)
                {
                    args.State = CoreWebView2PermissionState.Allow;
                }
            };

            // Gán Virtual Host Name để WebView2 chạy dưới origin HTTPS hợp lệ, cấp quyền Camera/Mic an toàn
            wvVideoCall.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "app.chatting.local",
                assetsPath,
                CoreWebView2HostResourceAccessKind.Allow);

            wvVideoCall.CoreWebView2.WebMessageReceived += HandleWebMessageFromJs;
            wvVideoCall.NavigationCompleted += (s, args) =>
            {
                if (args.IsSuccess)
                {
                    _isWebViewReady = true;
                    SendInitToJs();
                }
            };
            wvVideoCall.CoreWebView2.Navigate("https://app.chatting.local/webrtc_call.html");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khởi tạo WebView2: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void HandleWebMessageFromJs(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            // JavaScript posts objects, not only strings. TryGetWebMessageAsString
            // throws for a JSON object on some WebView2 runtimes, which previously
            // prevented mediaReady, offer/answer and ICE messages from ever reaching C#.
            using JsonDocument envelope = JsonDocument.Parse(e.WebMessageAsJson);
            string messageJson = envelope.RootElement.ValueKind == JsonValueKind.String
                ? envelope.RootElement.GetString() ?? "{}"
                : envelope.RootElement.GetRawText();
            using JsonDocument doc = JsonDocument.Parse(messageJson);
            var root = doc.RootElement;
            string type = root.TryGetProperty("type", out var typeProp) ? typeProp.GetString() ?? "" : "";

                if (type == "ready")
                {
                    _isWebViewReady = true;
                    SendInitToJs();
                }
                else if (type == "mediaReady")
                {
                    _isMediaReady = true;
                    await AnnouncePeerReadyAsync();

                    // Xử lý các tín hiệu WebRTC đã nhận khi camera/mic đang chuẩn bị.
                    FlushPendingSignals();
                    TryStartOffer();
                }
                else if (type == "signal")
                {
                    if (root.TryGetProperty("payload", out var payloadElem))
                    {
                        string sigType = payloadElem.TryGetProperty("type", out var stProp) ? stProp.GetString() ?? "" : "";
                        string sigData = payloadElem.TryGetProperty("data", out var sdProp) ? sdProp.GetString() ?? "" : "";

                        Guid target = _isInitiator
                            ? (_callInfo.ReceiverId ?? Guid.Empty)
                            : _callInfo.CallerId;

                        await _signalR.SendWebRtcSignalAsync(new WebRtcSignalDto
                        {
                            CallId = _callInfo.CallId,
                            TargetUserId = target,
                            SignalType = sigType,
                            Payload = sigData
                        });
                    }
                }
                else if (type == "status")
                {
                    string status = root.TryGetProperty("payload", out var pProp) ? pProp.GetString() ?? "" : "";
                    if (status == "connected")
                    {
                        _isConnected = true;
                        SoundNotificationService.Instance.StopCallingTone();
                        StopTimeoutTimer();

                        lblCallInfo.Text = $"Đã kết nối • {(_callInfo.CallType == CallType.Video ? "Video" : "Thoại")}";
                        lblCallInfo.ForeColor = Color.FromArgb(34, 197, 94);
                        timerCallDuration.Start();
                    }
                    else if (status == "disconnected")
                    {
                        lblCallInfo.Text = "Cuộc gọi bị gián đoạn";
                        lblCallInfo.ForeColor = Color.FromArgb(239, 68, 68);
                    }
                    else if (status == "camera_unavailable")
                    {
                        btnToggleCam.Visible = false;
                        lblCallInfo.Text = "Không tìm thấy camera (Đã chuyển sang thoại)";
                    }
                }
            else if (type == "error")
            {
                string errorMsg = root.TryGetProperty("payload", out var pProp) ? pProp.GetString() ?? "Lỗi thiết bị" : "Lỗi thiết bị";
                lblCallInfo.Text = "Lỗi thiết bị";
                lblCallInfo.ForeColor = Color.FromArgb(239, 68, 68);
                MessageBox.Show(errorMsg, "Lỗi Thiết Bị", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[VideoCallForm] JS error: {ex.Message}");
        }
    }

    private void SendInitToJs()
    {
        if (_isInitSent) return;
        if (wvVideoCall?.CoreWebView2 == null) return;
        _isInitSent = true;

        string remoteUser = _isInitiator
            ? (!string.IsNullOrWhiteSpace(_callInfo.ReceiverUsername) ? _callInfo.ReceiverUsername : "Người nhận")
            : _callInfo.CallerUsername;

        bool isVideo = _callInfo.CallType == CallType.Video;

        SendToJs(new
        {
            action = "init",
            initiator = _isInitiator,
            isVideo = isVideo,
            remoteName = remoteUser,
            remoteAvatar = _remoteAvatarDataUrl
        });
    }

    private void TriggerStartOffer()
    {
        SendToJs(new { action = "startOffer" });
    }

    private async Task AnnouncePeerReadyAsync()
    {
        if (_localPeerReady || _isCallEnded) return;

        _localPeerReady = true;
        Guid target = _isInitiator ? (_callInfo.ReceiverId ?? Guid.Empty) : _callInfo.CallerId;
        if (target == Guid.Empty) return;

        try
        {
            await _signalR.SendWebRtcSignalAsync(new WebRtcSignalDto
            {
                CallId = _callInfo.CallId,
                TargetUserId = target,
                SignalType = "peer-ready"
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[VideoCallForm] Could not send peer-ready: {ex.Message}");
        }
    }

    private void FlushPendingSignals()
    {
        while (_pendingSignals.Count > 0)
        {
            ProcessIncomingSignal(_pendingSignals.Dequeue());
        }
    }

    private void ProcessIncomingSignal(WebRtcSignalDto dto)
    {
        if (dto.SignalType == "peer-ready")
        {
            _remotePeerReady = true;
            // Receiving this from the callee is also proof that the call was accepted,
            // even if the CallAccepted broadcast arrived while the caller UI was loading.
            if (_isInitiator) _isCallAccepted = true;
            TryStartOffer();
            return;
        }

        SendSignalToJs(dto);
    }

    private void TryStartOffer()
    {
        if (!_isInitiator || !_isMediaReady || !_remotePeerReady || _offerStarted || _isCallEnded)
            return;

        _offerStarted = true;
        TriggerStartOffer();
    }

    private void SendSignalToJs(WebRtcSignalDto sig)
    {
        SendToJs(new
        {
            action = "signal",
            signal = new { type = sig.SignalType, data = sig.Payload }
        });
    }

    private void HandleIncomingSignal(WebRtcSignalDto dto)
    {
        RunOnUiThread(() => HandleIncomingSignalOnUiThread(dto));
    }

    private void HandleIncomingSignalOnUiThread(WebRtcSignalDto dto)
    {
        if (dto.CallId != _callInfo.CallId) return;

        if (!_isMediaReady)
        {
            _pendingSignals.Enqueue(dto);
            return;
        }

        ProcessIncomingSignal(dto);
    }

    private void HandleCallAccepted(CallInfoDto acceptedCall)
    {
        RunOnUiThread(() => HandleCallAcceptedOnUiThread(acceptedCall));
    }

    private void HandleCallAcceptedOnUiThread(CallInfoDto acceptedCall)
    {
        if (acceptedCall.CallId != _callInfo.CallId) return;
        if (_isCallAccepted) return;

        _isCallAccepted = true;
        SoundNotificationService.Instance.StopCallingTone();
        StopTimeoutTimer();

        lblCallInfo.Text = "Đang kết nối...";
        lblCallInfo.ForeColor = Color.FromArgb(56, 189, 248);

        // Offer is deliberately delayed until the callee's peer-ready arrives.
        TryStartOffer();
    }

    private void HandleCallEnded(CallEndedDto dto)
    {
        RunOnUiThread(() => HandleCallEndedOnUiThread(dto));
    }

    private void HandleCallEndedOnUiThread(CallEndedDto dto)
    {
        if (dto.CallId != _callInfo.CallId || _isCallEnded) return;

        _isCallEnded = true;
        SoundNotificationService.Instance.StopCallingTone();
        StopTimeoutTimer();
        timerCallDuration.Stop();

        SendToJs(new { action = "end" });

        if (dto.FinalStatus == CallStatus.Rejected)
        {
            lblCallInfo.Text = "Người nhận đã từ chối cuộc gọi";
            lblCallInfo.ForeColor = Color.FromArgb(239, 68, 68);
            MessageBox.Show("Người nhận đã từ chối cuộc gọi.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            lblCallInfo.Text = "Cuộc gọi đã kết thúc";
            lblCallInfo.ForeColor = Color.FromArgb(148, 163, 184);
        }

        Close();
    }

    private void RunOnUiThread(Action action)
    {
        if (IsDisposed || Disposing) return;
        if (!InvokeRequired)
        {
            action();
            return;
        }

        if (!IsHandleCreated) return;
        try
        {
            BeginInvoke(new Action(() =>
            {
                if (!IsDisposed && !Disposing) action();
            }));
        }
        catch (InvalidOperationException) { }
    }

    private void SendToJs(object obj)
    {
        if (wvVideoCall?.CoreWebView2 != null && _isWebViewReady)
        {
            try
            {
                wvVideoCall.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(obj));
            }
            catch { }
        }
    }

    private void timerCallDuration_Tick(object? sender, EventArgs e)
    {
        _secondsElapsed++;
        TimeSpan ts = TimeSpan.FromSeconds(_secondsElapsed);
        lblDuration.Text = ts.ToString(@"mm\:ss");
    }

    private void btnToggleMic_Click(object? sender, EventArgs e)
    {
        _isMicOn = !_isMicOn;
        UpdateMicrophoneButton();
        SendToJs(new { action = "toggleMic", enabled = _isMicOn });
    }

    private void btnToggleCam_Click(object? sender, EventArgs e)
    {
        _isCamOn = !_isCamOn;
        UpdateCameraButton();
        SendToJs(new { action = "toggleCam", enabled = _isCamOn });
    }

    private void UpdateMicrophoneButton()
    {
        string action = _isMicOn ? "Tắt" : "Bật";
        btnToggleMic.Text = $"  {action} mic";
        btnToggleMic.AccessibleName = $"{action} mic";
        btnToggleMic.AccessibleDescription = _isMicOn ? "Tắt micrô của bạn." : "Bật micrô của bạn.";
        btnToggleMic.BackColor = _isMicOn ? Color.FromArgb(30, 41, 59) : Color.FromArgb(239, 68, 68);
        string glyph = _isMicOn ? IconHelper.Mic : IconHelper.MicOff;
        IconHelper.SetButtonIcon(btnToggleMic, glyph, 14, Color.White, ContentAlignment.MiddleLeft);
        _controlToolTip.SetToolTip(btnToggleMic, _isMicOn ? "Tắt micrô" : "Bật micrô");
    }

    private void UpdateCameraButton()
    {
        string action = _isCamOn ? "Tắt" : "Bật";
        btnToggleCam.Text = $"  {action} cam";
        btnToggleCam.AccessibleName = $"{action} camera";
        btnToggleCam.AccessibleDescription = _isCamOn ? "Tắt camera của bạn." : "Bật camera của bạn.";
        btnToggleCam.BackColor = _isCamOn ? Color.FromArgb(30, 41, 59) : Color.FromArgb(239, 68, 68);
        string glyph = _isCamOn ? IconHelper.Video : IconHelper.VideoOff;
        IconHelper.SetButtonIcon(btnToggleCam, glyph, 14, Color.White, ContentAlignment.MiddleLeft);
        _controlToolTip.SetToolTip(btnToggleCam, _isCamOn ? "Tắt camera" : "Bật camera");
    }

    private async void btnEndCall_Click(object? sender, EventArgs e)
    {
        btnEndCall.Enabled = false;
        _isCallEnded = true;
        SoundNotificationService.Instance.StopCallingTone();
        StopTimeoutTimer();
        timerCallDuration.Stop();
        SendToJs(new { action = "end" });

        try
        {
            await _signalR.EndCallAsync(_callInfo.CallId);
        }
        catch { }

        Close();
    }

    private void VideoCallForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _signalR.OnReceiveWebRtcSignal -= HandleIncomingSignal;
        _signalR.OnCallAccepted -= HandleCallAccepted;
        _signalR.OnCallEnded -= HandleCallEnded;

        SoundNotificationService.Instance.StopCallingTone();
        StopTimeoutTimer();
        timerCallDuration.Stop();

        SendToJs(new { action = "end" });

        if (!_isCallEnded)
        {
            _isCallEnded = true;
            _ = _signalR.EndCallAsync(_callInfo.CallId);
        }
    }
}
