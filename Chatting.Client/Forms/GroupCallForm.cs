using Chatting.Client.Services;
using Chatting.Shared.DTOs;
using Chatting.Shared.Enums;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Text.Json;

namespace Chatting.Client.Forms;

/// <summary>LAN group call window. Each participant maintains one WebRTC peer per member.</summary>
public sealed class GroupCallForm : Form
{
    private readonly CallInfoDto _call;
    private readonly SignalRService _signalR;
    private readonly SessionManager _session;
    private readonly WebView2 _webView = new() { Dock = DockStyle.Fill, DefaultBackgroundColor = Color.FromArgb(15, 23, 42) };
    private readonly Label _status = new() { AutoEllipsis = true, ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Text = "Đang chuẩn bị cuộc gọi nhóm..." };
    private readonly Button _mic = new() { Width = 120, Dock = DockStyle.Right, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White };
    private readonly Button _camera = new() { Width = 120, Dock = DockStyle.Right, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White };
    private readonly Button _end = new() { Text = "  Rời cuộc gọi", Width = 140, Dock = DockStyle.Right, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White };
    private readonly Button _rejoin = new() { Text = "  Tham gia lại", Width = 155, Dock = DockStyle.Right, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, Visible = false };
    private readonly ToolTip _toolTip = new();
    private readonly List<GroupCallParticipantDto> _waitingParticipants = [];
    private readonly List<WebRtcSignalDto> _waitingSignals = [];
    private bool _webReady;
    private bool _isInCall = true;
    private bool _leaveRequestInProgress;
    private bool _joinRequestInProgress;
    private bool _callEnded;
    private bool _closing;
    private bool _micOn = true;
    private bool _cameraOn = true;

    public GroupCallForm(CallInfoDto call, SignalRService signalR, SessionManager session)
    {
        _call = call;
        _signalR = signalR;
        _session = session;

        Text = call.CallType == CallType.Video ? "Cuộc gọi nhóm video - Chatting" : "Cuộc gọi nhóm thoại - Chatting";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(720, 500);
        Size = new Size(1000, 700);
        BackColor = Color.FromArgb(15, 23, 42);

        var controls = new Panel { Dock = DockStyle.Bottom, Height = 72, Padding = new Padding(20, 14, 20, 14), BackColor = Color.FromArgb(30, 41, 59) };
        controls.Controls.Add(_status);
        controls.Controls.Add(_end);
        controls.Controls.Add(_rejoin);
        if (call.CallType == CallType.Video) controls.Controls.Add(_camera);
        controls.Controls.Add(_mic);
        Controls.Add(_webView);
        Controls.Add(controls);

        IconHelper.SetButtonIcon(_end, IconHelper.PhoneOff, 14, Color.White, ContentAlignment.MiddleLeft);
        IconHelper.SetButtonIcon(_rejoin, IconHelper.Refresh, 14, Color.White, ContentAlignment.MiddleLeft);

        _toolTip.ShowAlways = true;
        _end.AccessibleName = "Rời cuộc gọi nhóm";
        _end.AccessibleDescription = "Rời cuộc gọi nhóm hiện tại.";
        _rejoin.AccessibleName = "Tham gia lại cuộc gọi nhóm";
        _rejoin.AccessibleDescription = "Tham gia lại khi cuộc gọi nhóm vẫn đang diễn ra.";
        _toolTip.SetToolTip(_end, "Rời cuộc gọi nhóm");
        _toolTip.SetToolTip(_rejoin, "Tham gia lại cuộc gọi nhóm");
        UpdateMicrophoneButton();
        UpdateCameraButton();

        _mic.Click += (_, _) => { _micOn = !_micOn; UpdateMicrophoneButton(); Send(new { action = "toggleMic", enabled = _micOn }); };
        _camera.Click += (_, _) => { _cameraOn = !_cameraOn; UpdateCameraButton(); Send(new { action = "toggleCam", enabled = _cameraOn }); };
        _end.Click += async (_, _) => await LeaveAsync();
        _rejoin.Click += async (_, _) => await JoinAgainAsync();
        Load += GroupCallForm_Load;
        FormClosing += GroupCallForm_FormClosing;
        FormClosed += (_, _) => _toolTip.Dispose();
    }

    private void UpdateMicrophoneButton()
    {
        string action = _micOn ? "Tắt" : "Bật";
        _mic.Text = $"  {action} mic";
        _mic.AccessibleName = $"{action} mic";
        _mic.AccessibleDescription = _micOn ? "Tắt micrô của bạn." : "Bật micrô của bạn.";
        _toolTip.SetToolTip(_mic, _micOn ? "Tắt micrô" : "Bật micrô");
        _mic.BackColor = _micOn ? Color.FromArgb(30, 41, 59) : Color.FromArgb(239, 68, 68);
        string glyph = _micOn ? IconHelper.Mic : IconHelper.MicOff;
        IconHelper.SetButtonIcon(_mic, glyph, 14, Color.White, ContentAlignment.MiddleLeft);
    }

    private void UpdateCameraButton()
    {
        string action = _cameraOn ? "Tắt" : "Bật";
        _camera.Text = $"  {action} cam";
        _camera.AccessibleName = $"{action} camera";
        _camera.AccessibleDescription = _cameraOn ? "Tắt camera của bạn." : "Bật camera của bạn.";
        _toolTip.SetToolTip(_camera, _cameraOn ? "Tắt camera" : "Bật camera");
        _camera.BackColor = _cameraOn ? Color.FromArgb(30, 41, 59) : Color.FromArgb(239, 68, 68);
        string glyph = _cameraOn ? IconHelper.Video : IconHelper.VideoOff;
        IconHelper.SetButtonIcon(_camera, glyph, 14, Color.White, ContentAlignment.MiddleLeft);
    }

    private async void GroupCallForm_Load(object? sender, EventArgs e)
    {
        _signalR.OnReceiveWebRtcSignal += HandleSignal;
        _signalR.OnGroupCallParticipants += HandleRoster;
        _signalR.OnGroupCallParticipantJoined += HandleJoined;
        _signalR.OnGroupCallParticipantLeft += HandleLeft;
        _signalR.OnCallEnded += HandleCallEnded;
        await InitializeWebViewAsync();
    }

    private async Task InitializeWebViewAsync()
    {
        try
        {
            string assets = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
            if (!File.Exists(Path.Combine(assets, "group_call.html"))) throw new FileNotFoundException("Không tìm thấy Assets/group_call.html");
            string unsafeOrigin = Uri.TryCreate(_session.ServerUrl, UriKind.Absolute, out Uri? uri) && uri.Scheme == Uri.UriSchemeHttp
                ? " --unsafely-treat-insecure-origin-as-secure=" + uri.GetLeftPart(UriPartial.Authority) : string.Empty;
            var options = new CoreWebView2EnvironmentOptions("--disable-features=WebRtcHideLocalIpsWithMdns --force-webrtc-ip-handling-policy=default_public_and_private_interfaces --autoplay-policy=no-user-gesture-required --use-fake-ui-for-media-stream" + unsafeOrigin);
            string dataFolder = Path.Combine(Path.GetTempPath(), "Chatting_WebView2_Group_" + (_session.CurrentUser?.Id ?? Guid.NewGuid()));
            var environment = await CoreWebView2Environment.CreateAsync(null, dataFolder, options);
            await _webView.EnsureCoreWebView2Async(environment);
            _webView.CoreWebView2.PermissionRequested += (_, args) =>
            {
                if (args.PermissionKind is CoreWebView2PermissionKind.Camera or CoreWebView2PermissionKind.Microphone)
                    args.State = CoreWebView2PermissionState.Allow;
            };
            _webView.CoreWebView2.SetVirtualHostNameToFolderMapping("app.chatting.local", assets, CoreWebView2HostResourceAccessKind.Allow);
            _webView.CoreWebView2.WebMessageReceived += HandleWebMessage;
            _webView.CoreWebView2.Navigate("https://app.chatting.local/group_call.html");
        }
        catch (Exception ex)
        {
            _status.Text = "Không thể khởi tạo cuộc gọi nhóm";
            MessageBox.Show(ex.Message, "Lỗi cuộc gọi nhóm", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void HandleWebMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(e.WebMessageAsJson);
            JsonElement root = doc.RootElement.ValueKind == JsonValueKind.String ? JsonDocument.Parse(doc.RootElement.GetString() ?? "{}").RootElement.Clone() : doc.RootElement.Clone();
            string type = root.TryGetProperty("type", out var t) ? t.GetString() ?? "" : "";
            if (type == "ready")
            {
                _webReady = true;
                if (!_isInCall)
                {
                    _waitingParticipants.Clear();
                    _waitingSignals.Clear();
                    Send(new { action = "end" });
                    return;
                }

                var me = _session.CurrentUser!;
                Send(new { action = "init", callType = _call.CallType.ToString(), self = new { userId = me.Id, username = me.Username, displayName = me.DisplayName } });
                foreach (var participant in _waitingParticipants.ToList()) AddParticipant(participant);
                _waitingParticipants.Clear();
                foreach (var signal in _waitingSignals.ToList()) SendSignalToWebView(signal);
                _waitingSignals.Clear();
            }
            else if (type == "mediaReady")
            {
                if (_isInCall) _status.Text = "Đang chờ các thành viên tham gia...";
                else Send(new { action = "end" });
            }
            else if (type == "signal" && root.TryGetProperty("payload", out var payload))
            {
                Guid target = payload.GetProperty("targetUserId").GetGuid();
                await _signalR.SendWebRtcSignalAsync(new WebRtcSignalDto
                {
                    CallId = _call.CallId,
                    TargetUserId = target,
                    SignalType = payload.GetProperty("signalType").GetString() ?? "",
                    Payload = payload.GetProperty("data").GetString() ?? ""
                });
            }
            else if (type == "connected" && _isInCall) _status.Text = "Đã kết nối cuộc gọi nhóm";
            else if (type == "error") _status.Text = "Lỗi thiết bị: " + (root.TryGetProperty("payload", out var p) ? p.GetString() : "");
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine("[GroupCall] " + ex.Message); }
    }

    private void HandleRoster(List<GroupCallParticipantDto> participants)
    {
        RunOnUiThread(() =>
        {
            if (!_isInCall) return;
            foreach (var participant in participants.Where(p => p.CallId == _call.CallId))
                AddParticipantOnUiThread(participant);
        });
    }

    private void HandleJoined(GroupCallParticipantDto participant)
    {
        RunOnUiThread(() =>
        {
            if (_isInCall && participant.CallId == _call.CallId && participant.UserId != _session.CurrentUser?.Id)
                AddParticipantOnUiThread(participant);
        });
    }

    private void HandleLeft(GroupCallParticipantDto participant)
    {
        RunOnUiThread(() =>
        {
            if (participant.CallId == _call.CallId)
            {
                _waitingParticipants.RemoveAll(p => p.UserId == participant.UserId);
                _waitingSignals.RemoveAll(s => s.SenderUserId == participant.UserId);
                Send(new { action = "removePeer", userId = participant.UserId });
            }
        });
    }

    private void AddParticipant(GroupCallParticipantDto participant)
    {
        RunOnUiThread(() => AddParticipantOnUiThread(participant));
    }

    private void AddParticipantOnUiThread(GroupCallParticipantDto participant)
    {
        if (participant.UserId == _session.CurrentUser?.Id) return;
        if (!_webReady) { if (_waitingParticipants.All(p => p.UserId != participant.UserId)) _waitingParticipants.Add(participant); return; }
        Send(new { action = "addPeer", userId = participant.UserId, name = string.IsNullOrWhiteSpace(participant.DisplayName) ? participant.Username : participant.DisplayName });
    }

    private void HandleSignal(WebRtcSignalDto signal)
    {
        RunOnUiThread(() =>
        {
            if (signal.CallId != _call.CallId || signal.TargetUserId != _session.CurrentUser?.Id) return;
            if (!_isInCall) return;
            if (!_webReady)
            {
                _waitingSignals.Add(signal);
                return;
            }
            SendSignalToWebView(signal);
        });
    }

    private void SendSignalToWebView(WebRtcSignalDto signal) =>
        Send(new { action = "signal", senderUserId = signal.SenderUserId, signalType = signal.SignalType, data = signal.Payload });

    private void HandleCallEnded(CallEndedDto ended)
    {
        RunOnUiThread(() =>
        {
            if (ended.CallId != _call.CallId) return;
            _callEnded = true;
            if (_leaveRequestInProgress) return;
            _closing = true;
            Close();
        });
    }

    private void Send(object message)
    {
        RunOnUiThread(() =>
        {
            if (!_webReady || _webView.IsDisposed) return;
            _webView.CoreWebView2?.PostWebMessageAsJson(JsonSerializer.Serialize(message));
        });
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
        try { BeginInvoke(action); }
        catch (InvalidOperationException) { }
    }

    private async Task LeaveAsync()
    {
        if (!_isInCall || _leaveRequestInProgress || _closing) return;
        _leaveRequestInProgress = true;
        _end.Enabled = false;
        _status.Text = "Đang rời cuộc gọi...";
        try
        {
            await _signalR.EndCallAsync(_call.CallId);
            if (IsDisposed || Disposing) return;

            _isInCall = false;
            _leaveRequestInProgress = false;
            Send(new { action = "end" });
            _waitingParticipants.Clear();
            _waitingSignals.Clear();

            if (_callEnded)
            {
                _closing = true;
                Close();
                return;
            }

            _end.Visible = false;
            _mic.Visible = false;
            _camera.Visible = false;
            _rejoin.Visible = true;
            _rejoin.Enabled = true;
            _status.Text = "Bạn đã rời cuộc gọi. Có thể tham gia lại khi cuộc gọi còn diễn ra.";
        }
        catch (Exception ex)
        {
            _leaveRequestInProgress = false;
            if (!IsDisposed && !Disposing)
            {
                _end.Enabled = true;
                _status.Text = "Không thể rời cuộc gọi: " + ex.Message;
            }
        }
    }

    private async Task JoinAgainAsync()
    {
        if (_isInCall || _joinRequestInProgress || _closing) return;
        _joinRequestInProgress = true;
        _isInCall = true;
        _rejoin.Enabled = false;
        _status.Text = "Đang tham gia lại cuộc gọi...";
        try
        {
            bool? accepted = await _signalR.AcceptCallAsync(_call.CallId);
            if (IsDisposed || Disposing)
            {
                if (accepted == true) await _signalR.EndCallAsync(_call.CallId);
                return;
            }
            if (accepted is null)
                throw new InvalidOperationException("Mất kết nối đến máy chủ. Vui lòng thử lại.");
            if (!accepted.Value || _callEnded)
            {
                _isInCall = false;
                _callEnded = true;
                _closing = true;
                Close();
                return;
            }

            _joinRequestInProgress = false;
            _rejoin.Visible = false;
            _end.Visible = true;
            _end.Enabled = true;
            _mic.Visible = true;
            _camera.Visible = _call.CallType == CallType.Video;
            _micOn = true;
            _cameraOn = true;
            UpdateMicrophoneButton();
            UpdateCameraButton();
            _status.Text = "Đang kết nối lại với các thành viên...";
            var me = _session.CurrentUser!;
            Send(new { action = "init", callType = _call.CallType.ToString(), self = new { userId = me.Id, username = me.Username, displayName = me.DisplayName } });
        }
        catch (Exception ex)
        {
            _joinRequestInProgress = false;
            _isInCall = false;
            if (!IsDisposed && !Disposing)
            {
                _rejoin.Enabled = true;
                _status.Text = "Không thể tham gia lại: " + ex.Message;
            }
        }
    }

    private void GroupCallForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _signalR.OnReceiveWebRtcSignal -= HandleSignal;
        _signalR.OnGroupCallParticipants -= HandleRoster;
        _signalR.OnGroupCallParticipantJoined -= HandleJoined;
        _signalR.OnGroupCallParticipantLeft -= HandleLeft;
        _signalR.OnCallEnded -= HandleCallEnded;
        _closing = true;
        if (_isInCall && !_leaveRequestInProgress) { _isInCall = false; _ = _signalR.EndCallAsync(_call.CallId); }
    }
}
