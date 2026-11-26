using Chatting.Client.Forms;
using Chatting.Client.Services;
using Chatting.Shared.DTOs;
using Chatting.Shared.Enums;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Chatting.Client.Controls;

/// <summary>
/// Panel hiển thị các nội dung đã chia sẻ trong cuộc trò chuyện (Ảnh & Video, Tệp, Liên kết, Tin nhắn thoại).
/// Thiết kế hiện đại, responsive, tương tự mục Media, Files & Links của Messenger.
/// </summary>
public class UcSharedContent : UserControl
{
    [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
    private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect,
        int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

    private readonly ApiClient _apiClient;
    private readonly SessionManager _session;
    private readonly ToolTip _toolTip = new();

    public event Action? OnCloseRequested;

    private Guid? _currentConversationId;
    private ConversationDto? _currentConversation;
    private SharedContentDto _sharedData = new();
    private string _activeTab = "media"; // "media", "files", "links", "voice"
    private string _fileSearchKeyword = string.Empty;

    // Header Controls
    private readonly Panel pnlHeader = new();
    private readonly Label lblTitle = new();
    private readonly Label lblSubtitle = new();
    private readonly Button btnClose = new();
    private readonly Button btnRefresh = new();

    // Tab Bar Controls
    private readonly Panel pnlTabBar = new();
    private readonly Button btnTabMedia = new();
    private readonly Button btnTabFiles = new();
    private readonly Button btnTabLinks = new();
    private readonly Button btnTabVoice = new();
    private readonly Panel pnlTabIndicator = new();

    // Body & Tab Containers
    private readonly Panel pnlBody = new();
    private readonly Panel pnlLoading = new();
    private readonly Label lblLoadingIcon = new();
    private readonly Label lblLoadingText = new();

    private readonly Panel pnlEmptyState = new();
    private readonly Label lblEmptyIcon = new();
    private readonly Label lblEmptyTitle = new();
    private readonly Label lblEmptySubtitle = new();

    // Tab 1: Media
    private readonly Panel pnlMediaTab = new();
    private readonly FlowLayoutPanel flpMedia = new();

    // Tab 2: Files
    private readonly Panel pnlFilesTab = new();
    private readonly Panel pnlFilesSearch = new();
    private readonly Panel pnlFilesSearchBox = new();
    private readonly Label lblFilesSearchIcon = new();
    private readonly TextBox txtSearchFiles = new();
    private readonly FlowLayoutPanel flpFiles = new();

    // Tab 3: Links
    private readonly Panel pnlLinksTab = new();
    private readonly FlowLayoutPanel flpLinks = new();

    // Tab 4: Voice
    private readonly Panel pnlVoiceTab = new();
    private readonly FlowLayoutPanel flpVoice = new();

    // Active voice tracking in panel
    private readonly Dictionary<Guid, (Button btnPlay, Panel pnlBar, Label lblTime)> _voiceControls = new();

    public UcSharedContent(ApiClient apiClient, SessionManager session)
    {
        _apiClient = apiClient;
        _session = session;

        InitializeComponentManual();

        // Hook Voice Player global events
        VoicePlayerManager.Instance.OnProgress += VoicePlayer_OnProgress;
        VoicePlayerManager.Instance.OnStateChanged += VoicePlayer_OnStateChanged;
        VoicePlayerManager.Instance.OnStopped += VoicePlayer_OnStopped;

        Disposed += (s, e) =>
        {
            VoicePlayerManager.Instance.OnProgress -= VoicePlayer_OnProgress;
            VoicePlayerManager.Instance.OnStateChanged -= VoicePlayer_OnStateChanged;
            VoicePlayerManager.Instance.OnStopped -= VoicePlayer_OnStopped;
            ClearThumbnails();
            _toolTip.Dispose();
        };
    }

    private void InitializeComponentManual()
    {
        Dock = DockStyle.Right;
        Width = 350;
        BackColor = Color.White;
        DoubleBuffered = true;

        // Draw left separator border line
        Paint += (s, pe) =>
        {
            using var pen = new Pen(Color.FromArgb(226, 232, 240), 1);
            pe.Graphics.DrawLine(pen, 0, 0, 0, Height);
        };

        // ── 1. Header ────────────────────────────────────────────────────────
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.Height = 60;
        pnlHeader.BackColor = Color.White;
        pnlHeader.Padding = new Padding(16, 10, 10, 10);

        lblTitle.Text = "Nội dung đã chia sẻ";
        lblTitle.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
        lblTitle.Location = new Point(16, 10);
        lblTitle.AutoSize = true;

        lblSubtitle.Text = "Ảnh, tệp tin & liên kết";
        lblSubtitle.Font = new Font("Segoe UI", 8F);
        lblSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
        lblSubtitle.Location = new Point(16, 32);
        lblSubtitle.AutoSize = true;

        btnClose.Text = "\uE711";
        btnClose.Font = new Font("Segoe MDL2 Assets", 10F, FontStyle.Regular);
        btnClose.Size = new Size(32, 32);
        btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnClose.Location = new Point(Width - 42, 14);
        btnClose.FlatStyle = FlatStyle.Flat;
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.BackColor = Color.Transparent;
        btnClose.ForeColor = Color.FromArgb(100, 116, 139);
        btnClose.Cursor = Cursors.Hand;
        btnClose.AccessibleName = "Đóng nội dung đã chia sẻ";
        btnClose.AccessibleDescription = "Đóng ngăn nội dung đã chia sẻ.";
        btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(226, 232, 240);
        _toolTip.SetToolTip(btnClose, "Đóng nội dung đã chia sẻ");
        btnClose.Click += (s, e) => OnCloseRequested?.Invoke();

        btnRefresh.Text = "\uE72C";
        btnRefresh.Font = new Font("Segoe MDL2 Assets", 10F);
        btnRefresh.Size = new Size(32, 32);
        btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnRefresh.Location = new Point(Width - 78, 14);
        btnRefresh.FlatStyle = FlatStyle.Flat;
        btnRefresh.FlatAppearance.BorderSize = 0;
        btnRefresh.BackColor = Color.Transparent;
        btnRefresh.ForeColor = Color.FromArgb(100, 116, 139);
        btnRefresh.Cursor = Cursors.Hand;
        btnRefresh.AccessibleName = "Làm mới nội dung đã chia sẻ";
        btnRefresh.AccessibleDescription = "Tải lại ảnh, tệp, liên kết và tin nhắn thoại.";
        btnRefresh.FlatAppearance.MouseOverBackColor = Color.FromArgb(226, 232, 240);
        _toolTip.SetToolTip(btnRefresh, "Làm mới nội dung đã chia sẻ");
        btnRefresh.Click += async (s, e) =>
        {
            if (_currentConversation != null)
                await LoadContentAsync(_currentConversation);
        };

        pnlHeader.Controls.Add(lblTitle);
        pnlHeader.Controls.Add(lblSubtitle);
        pnlHeader.Controls.Add(btnRefresh);
        pnlHeader.Controls.Add(btnClose);

        // Header bottom border
        pnlHeader.Paint += (s, pe) =>
        {
            using var pen = new Pen(Color.FromArgb(241, 245, 249), 1);
            pe.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
        };

        // ── 2. Tab Bar ───────────────────────────────────────────────────────
        pnlTabBar.Dock = DockStyle.Top;
        pnlTabBar.Height = 42;
        pnlTabBar.BackColor = Color.FromArgb(248, 250, 252);
        pnlTabBar.Padding = new Padding(6, 0, 6, 0);

        ConfigureTabButton(btnTabMedia, "Ảnh & Video", "media");
        ConfigureTabButton(btnTabFiles, "Tệp", "files");
        ConfigureTabButton(btnTabLinks, "Liên kết", "links");
        ConfigureTabButton(btnTabVoice, "Tin thoại", "voice");

        pnlTabIndicator.Height = 3;
        pnlTabIndicator.BackColor = Color.FromArgb(37, 99, 235);
        pnlTabIndicator.Visible = true;

        pnlTabBar.Controls.Add(btnTabMedia);
        pnlTabBar.Controls.Add(btnTabFiles);
        pnlTabBar.Controls.Add(btnTabLinks);
        pnlTabBar.Controls.Add(btnTabVoice);
        pnlTabBar.Controls.Add(pnlTabIndicator);

        pnlTabBar.Resize += (s, e) => LayoutTabBar();

        // ── 3. Body Panel ────────────────────────────────────────────────────
        pnlBody.Dock = DockStyle.Fill;
        pnlBody.BackColor = Color.White;

        // Loading view
        pnlLoading.Dock = DockStyle.Fill;
        pnlLoading.BackColor = Color.White;
        pnlLoading.Visible = false;

        lblLoadingIcon.Text = "\uE72C";
        lblLoadingIcon.Font = new Font("Segoe MDL2 Assets", 24F);
        lblLoadingIcon.ForeColor = Color.FromArgb(96, 165, 250);
        lblLoadingIcon.Size = new Size(50, 50);
        lblLoadingIcon.Location = new Point((Width - 50) / 2, 140);
        lblLoadingIcon.TextAlign = ContentAlignment.MiddleCenter;

        lblLoadingText.Text = "Đang tải nội dung chia sẻ...";
        lblLoadingText.Font = new Font("Segoe UI", 9.5F);
        lblLoadingText.ForeColor = Color.FromArgb(100, 116, 139);
        lblLoadingText.Size = new Size(300, 30);
        lblLoadingText.Location = new Point((Width - 300) / 2, 195);
        lblLoadingText.TextAlign = ContentAlignment.MiddleCenter;

        pnlLoading.Controls.Add(lblLoadingIcon);
        pnlLoading.Controls.Add(lblLoadingText);

        // Empty state view
        pnlEmptyState.Dock = DockStyle.Fill;
        pnlEmptyState.BackColor = Color.White;
        pnlEmptyState.Visible = false;

        lblEmptyIcon.Text = "\uE8B7";
        lblEmptyIcon.Font = new Font("Segoe MDL2 Assets", 34F);
        lblEmptyIcon.ForeColor = Color.FromArgb(148, 163, 184);
        lblEmptyIcon.Size = new Size(60, 60);
        lblEmptyIcon.Location = new Point((Width - 60) / 2, 130);
        lblEmptyIcon.TextAlign = ContentAlignment.MiddleCenter;

        lblEmptyTitle.Text = "Chưa có nội dung";
        lblEmptyTitle.Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold);
        lblEmptyTitle.ForeColor = Color.FromArgb(30, 41, 59);
        lblEmptyTitle.Size = new Size(300, 24);
        lblEmptyTitle.Location = new Point((Width - 300) / 2, 195);
        lblEmptyTitle.TextAlign = ContentAlignment.MiddleCenter;

        lblEmptySubtitle.Text = "Các nội dung chia sẻ sẽ xuất hiện ở đây";
        lblEmptySubtitle.Font = new Font("Segoe UI", 8.5F);
        lblEmptySubtitle.ForeColor = Color.FromArgb(148, 163, 184);
        lblEmptySubtitle.Size = new Size(320, 40);
        lblEmptySubtitle.Location = new Point((Width - 320) / 2, 222);
        lblEmptySubtitle.TextAlign = ContentAlignment.TopCenter;

        pnlEmptyState.Controls.Add(lblEmptyIcon);
        pnlEmptyState.Controls.Add(lblEmptyTitle);
        pnlEmptyState.Controls.Add(lblEmptySubtitle);

        // Tab 1: Media
        pnlMediaTab.Dock = DockStyle.Fill;
        pnlMediaTab.BackColor = Color.White;
        flpMedia.Dock = DockStyle.Fill;
        flpMedia.AutoScroll = true;
        flpMedia.Padding = new Padding(10, 8, 10, 8);
        flpMedia.BackColor = Color.White;
        pnlMediaTab.Controls.Add(flpMedia);

        // Tab 2: Files
        pnlFilesTab.Dock = DockStyle.Fill;
        pnlFilesTab.BackColor = Color.White;

        pnlFilesSearch.Dock = DockStyle.Top;
        pnlFilesSearch.Height = 46;
        pnlFilesSearch.BackColor = Color.White;
        pnlFilesSearch.Padding = new Padding(12, 6, 12, 6);

        pnlFilesSearchBox.Dock = DockStyle.Fill;
        pnlFilesSearchBox.BackColor = Color.FromArgb(241, 245, 249);
        pnlFilesSearchBox.Padding = new Padding(8, 5, 8, 5);

        lblFilesSearchIcon.Text = "\uE721";
        lblFilesSearchIcon.Font = new Font("Segoe MDL2 Assets", 9F);
        lblFilesSearchIcon.Location = new Point(6, 6);
        lblFilesSearchIcon.Size = new Size(20, 20);

        txtSearchFiles.BorderStyle = BorderStyle.None;
        txtSearchFiles.BackColor = Color.FromArgb(241, 245, 249);
        txtSearchFiles.Font = new Font("Segoe UI", 9F);
        txtSearchFiles.ForeColor = Color.FromArgb(15, 23, 42);
        txtSearchFiles.Location = new Point(28, 7);
        txtSearchFiles.Size = new Size(270, 18);
        txtSearchFiles.PlaceholderText = "Tìm kiếm theo tên tệp...";
        txtSearchFiles.TextChanged += (s, e) =>
        {
            _fileSearchKeyword = txtSearchFiles.Text.Trim();
            RenderFilesTab();
        };

        pnlFilesSearchBox.Controls.Add(lblFilesSearchIcon);
        pnlFilesSearchBox.Controls.Add(txtSearchFiles);
        pnlFilesSearch.Controls.Add(pnlFilesSearchBox);

        flpFiles.Dock = DockStyle.Fill;
        flpFiles.AutoScroll = true;
        flpFiles.FlowDirection = FlowDirection.TopDown;
        flpFiles.WrapContents = false;
        flpFiles.Padding = new Padding(8, 4, 8, 8);
        flpFiles.BackColor = Color.White;

        pnlFilesTab.Controls.Add(flpFiles);
        pnlFilesTab.Controls.Add(pnlFilesSearch);

        // Tab 3: Links
        pnlLinksTab.Dock = DockStyle.Fill;
        pnlLinksTab.BackColor = Color.White;
        flpLinks.Dock = DockStyle.Fill;
        flpLinks.AutoScroll = true;
        flpLinks.FlowDirection = FlowDirection.TopDown;
        flpLinks.WrapContents = false;
        flpLinks.Padding = new Padding(8, 8, 8, 8);
        flpLinks.BackColor = Color.White;
        pnlLinksTab.Controls.Add(flpLinks);

        // Tab 4: Voice
        pnlVoiceTab.Dock = DockStyle.Fill;
        pnlVoiceTab.BackColor = Color.White;
        flpVoice.Dock = DockStyle.Fill;
        flpVoice.AutoScroll = true;
        flpVoice.FlowDirection = FlowDirection.TopDown;
        flpVoice.WrapContents = false;
        flpVoice.Padding = new Padding(8, 8, 8, 8);
        flpVoice.BackColor = Color.White;
        pnlVoiceTab.Controls.Add(flpVoice);

        // Add view panels to body
        pnlBody.Controls.Add(pnlLoading);
        pnlBody.Controls.Add(pnlEmptyState);
        pnlBody.Controls.Add(pnlMediaTab);
        pnlBody.Controls.Add(pnlFilesTab);
        pnlBody.Controls.Add(pnlLinksTab);
        pnlBody.Controls.Add(pnlVoiceTab);

        // Assemble Main UserControl
        Controls.Add(pnlBody);
        Controls.Add(pnlTabBar);
        Controls.Add(pnlHeader);

        // Apply initial layout & active tab
        LayoutTabBar();
        SwitchTab("media");
    }

    private void ConfigureTabButton(Button btn, string text, string tabKey)
    {
        btn.Text = text;
        btn.Tag = tabKey;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.BackColor = Color.Transparent;
        btn.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
        btn.ForeColor = Color.FromArgb(100, 116, 139);
        btn.Cursor = Cursors.Hand;
        btn.Click += (s, e) => SwitchTab(tabKey);
    }

    private void LayoutTabBar()
    {
        int totalWidth = pnlTabBar.ClientSize.Width - pnlTabBar.Padding.Horizontal;
        int tabCount = 4;
        int tabWidth = Math.Max(50, totalWidth / tabCount);

        btnTabMedia.SetBounds(pnlTabBar.Padding.Left, 0, tabWidth, pnlTabBar.Height - 3);
        btnTabFiles.SetBounds(btnTabMedia.Right, 0, tabWidth, pnlTabBar.Height - 3);
        btnTabLinks.SetBounds(btnTabFiles.Right, 0, tabWidth, pnlTabBar.Height - 3);
        btnTabVoice.SetBounds(btnTabLinks.Right, 0, totalWidth - (tabWidth * 3), pnlTabBar.Height - 3);

        UpdateTabIndicator();
    }

    private void SwitchTab(string tabKey)
    {
        _activeTab = tabKey;

        // Button styles
        foreach (var btn in new[] { btnTabMedia, btnTabFiles, btnTabLinks, btnTabVoice })
        {
            bool isActive = (string?)btn.Tag == tabKey;
            btn.ForeColor = isActive ? Color.FromArgb(37, 99, 235) : Color.FromArgb(100, 116, 139);
            btn.Font = new Font("Segoe UI Semibold", 8.5F, isActive ? FontStyle.Bold : FontStyle.Regular);
        }

        UpdateTabIndicator();

        // Show corresponding tab view
        pnlMediaTab.Visible = tabKey == "media";
        pnlFilesTab.Visible = tabKey == "files";
        pnlLinksTab.Visible = tabKey == "links";
        pnlVoiceTab.Visible = tabKey == "voice";

        // Check empty state for active tab
        CheckActiveTabEmptyState();
    }

    private void UpdateTabIndicator()
    {
        Button? targetBtn = _activeTab switch
        {
            "media" => btnTabMedia,
            "files" => btnTabFiles,
            "links" => btnTabLinks,
            "voice" => btnTabVoice,
            _ => btnTabMedia
        };

        if (targetBtn != null)
        {
            pnlTabIndicator.SetBounds(targetBtn.Left + 8, pnlTabBar.Height - 3, Math.Max(20, targetBtn.Width - 16), 3);
            pnlTabIndicator.BringToFront();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Loading & Refreshing Data
    // ─────────────────────────────────────────────────────────────────────────
    public async Task LoadContentAsync(ConversationDto conv)
    {
        _currentConversationId = conv.Id;
        _currentConversation = conv;

        lblSubtitle.Text = $"với {conv.Title}";
        ClearThumbnails();
        _voiceControls.Clear();

        ShowLoading(true);

        try
        {
            // Call dedicated server endpoint
            var res = await _apiClient.GetSharedContentAsync(conv.Id);
            if (res?.Success == true && res.Data != null)
            {
                _sharedData = res.Data;
            }
            else
            {
                // Fallback: parse from messages if server endpoint is unavailable
                _sharedData = await FallbackExtractFromMessagesAsync(conv.Id);
            }
        }
        catch
        {
            _sharedData = await FallbackExtractFromMessagesAsync(conv.Id);
        }
        finally
        {
            ShowLoading(false);
        }

        UpdateTabBadges();
        RenderAllTabs();
        CheckActiveTabEmptyState();
    }

    private async Task<SharedContentDto> FallbackExtractFromMessagesAsync(Guid convId)
    {
        var result = new SharedContentDto();
        try
        {
            var res = await _apiClient.GetMessagesAsync(convId, 1, 100);
            if (res?.Success == true && res.Data?.Items != null)
            {
                var linkRegex = new Regex(@"(https?://[^\s]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                foreach (var m in res.Data.Items.Where(x => !x.IsDeleted && !x.IsRevoked))
                {
                    if (m.Attachments.Count > 0)
                    {
                        foreach (var att in m.Attachments)
                        {
                            string ext = Path.GetExtension(att.OriginalFileName).ToLowerInvariant();
                            string ct = (att.ContentType ?? string.Empty).ToLowerInvariant();

                            bool isImg = m.Type == MessageType.Image ||
                                         ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp" ||
                                         ct.StartsWith("image/");

                            bool isVid = m.Type == MessageType.Video ||
                                         ext is ".mp4" or ".mov" or ".avi" or ".mkv" or ".webm" ||
                                         ct.StartsWith("video/");

                            bool isVoi = m.Type == MessageType.Voice ||
                                         ext is ".wav" or ".ogg" or ".m4a" ||
                                         ct.StartsWith("audio/");

                            if (isImg || isVid)
                            {
                                result.Media.Add(new SharedMediaItemDto
                                {
                                    AttachmentId = att.Id,
                                    MessageId = m.Id,
                                    FileName = att.OriginalFileName,
                                    FileSize = att.FileSize,
                                    ContentType = att.ContentType ?? string.Empty,
                                    RelativePath = att.RelativePath,
                                    IsVideo = isVid,
                                    SenderId = m.SenderId,
                                    SenderUsername = m.SenderUsername,
                                    CreatedAt = m.CreatedAt
                                });
                            }
                            else if (isVoi)
                            {
                                result.VoiceMessages.Add(new SharedVoiceItemDto
                                {
                                    AttachmentId = att.Id,
                                    MessageId = m.Id,
                                    FileName = att.OriginalFileName,
                                    DurationSeconds = att.DurationSeconds ?? 0,
                                    SenderId = m.SenderId,
                                    SenderUsername = m.SenderUsername,
                                    CreatedAt = m.CreatedAt
                                });
                            }
                            else
                            {
                                result.Files.Add(new SharedFileItemDto
                                {
                                    AttachmentId = att.Id,
                                    MessageId = m.Id,
                                    FileName = att.OriginalFileName,
                                    FileSize = att.FileSize,
                                    ContentType = att.ContentType ?? string.Empty,
                                    RelativePath = att.RelativePath,
                                    SenderId = m.SenderId,
                                    SenderUsername = m.SenderUsername,
                                    CreatedAt = m.CreatedAt
                                });
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(m.Content))
                    {
                        var matches = linkRegex.Matches(m.Content);
                        foreach (Match match in matches)
                        {
                            string url = match.Value.TrimEnd('.', ',', ';', '!', '?', ')');
                            if (Uri.TryCreate(url, UriKind.Absolute, out _))
                            {
                                result.Links.Add(new SharedLinkItemDto
                                {
                                    MessageId = m.Id,
                                    Url = url,
                                    SenderId = m.SenderId,
                                    SenderUsername = m.SenderUsername,
                                    CreatedAt = m.CreatedAt
                                });
                            }
                        }
                    }
                }
            }
        }
        catch { }

        return result;
    }

    public void OnNewMessage(MessageDto msg)
    {
        if (msg.ConversationId != _currentConversationId) return;

        bool updated = false;

        // Process attachments
        if (msg.Attachments.Count > 0 && !msg.IsDeleted && !msg.IsRevoked)
        {
            foreach (var att in msg.Attachments)
            {
                string ext = Path.GetExtension(att.OriginalFileName).ToLowerInvariant();
                string ct = (att.ContentType ?? string.Empty).ToLowerInvariant();

                bool isImg = msg.Type == MessageType.Image ||
                             ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp" ||
                             ct.StartsWith("image/");

                bool isVid = msg.Type == MessageType.Video ||
                             ext is ".mp4" or ".mov" or ".avi" or ".mkv" or ".webm" ||
                             ct.StartsWith("video/");

                bool isVoi = msg.Type == MessageType.Voice ||
                             ext is ".wav" or ".ogg" or ".m4a" ||
                             ct.StartsWith("audio/");

                if (isImg || isVid)
                {
                    _sharedData.Media.Insert(0, new SharedMediaItemDto
                    {
                        AttachmentId = att.Id,
                        MessageId = msg.Id,
                        FileName = att.OriginalFileName,
                        FileSize = att.FileSize,
                        ContentType = att.ContentType ?? string.Empty,
                        RelativePath = att.RelativePath,
                        IsVideo = isVid,
                        SenderId = msg.SenderId,
                        SenderUsername = msg.SenderUsername,
                        CreatedAt = msg.CreatedAt
                    });
                    updated = true;
                }
                else if (isVoi)
                {
                    _sharedData.VoiceMessages.Insert(0, new SharedVoiceItemDto
                    {
                        AttachmentId = att.Id,
                        MessageId = msg.Id,
                        FileName = att.OriginalFileName,
                        DurationSeconds = att.DurationSeconds ?? 0,
                        SenderId = msg.SenderId,
                        SenderUsername = msg.SenderUsername,
                        CreatedAt = msg.CreatedAt
                    });
                    updated = true;
                }
                else
                {
                    _sharedData.Files.Insert(0, new SharedFileItemDto
                    {
                        AttachmentId = att.Id,
                        MessageId = msg.Id,
                        FileName = att.OriginalFileName,
                        FileSize = att.FileSize,
                        ContentType = att.ContentType ?? string.Empty,
                        RelativePath = att.RelativePath,
                        SenderId = msg.SenderId,
                        SenderUsername = msg.SenderUsername,
                        CreatedAt = msg.CreatedAt
                    });
                    updated = true;
                }
            }
        }

        // Process links
        if (!string.IsNullOrWhiteSpace(msg.Content) && !msg.IsDeleted && !msg.IsRevoked)
        {
            var linkRegex = new Regex(@"(https?://[^\s]+)", RegexOptions.IgnoreCase);
            var matches = linkRegex.Matches(msg.Content);
            foreach (Match match in matches)
            {
                string url = match.Value.TrimEnd('.', ',', ';', '!', '?', ')');
                if (Uri.TryCreate(url, UriKind.Absolute, out _))
                {
                    _sharedData.Links.Insert(0, new SharedLinkItemDto
                    {
                        MessageId = msg.Id,
                        Url = url,
                        SenderId = msg.SenderId,
                        SenderUsername = msg.SenderUsername,
                        CreatedAt = msg.CreatedAt
                    });
                    updated = true;
                }
            }
        }

        if (updated)
        {
            UpdateTabBadges();
            RenderAllTabs();
            CheckActiveTabEmptyState();
        }
    }

    private void UpdateTabBadges()
    {
        btnTabMedia.Text = _sharedData.Media.Count > 0 ? $"Ảnh & Video ({_sharedData.Media.Count})" : "Ảnh & Video";
        btnTabFiles.Text = _sharedData.Files.Count > 0 ? $"Tệp ({_sharedData.Files.Count})" : "Tệp";
        btnTabLinks.Text = _sharedData.Links.Count > 0 ? $"Liên kết ({_sharedData.Links.Count})" : "Liên kết";
        btnTabVoice.Text = _sharedData.VoiceMessages.Count > 0 ? $"Voice ({_sharedData.VoiceMessages.Count})" : "Voice";

        LayoutTabBar();
    }

    private void ShowLoading(bool loading)
    {
        pnlLoading.Visible = loading;
        if (loading)
        {
            pnlLoading.BringToFront();
            pnlEmptyState.Visible = false;
        }
    }

    private void CheckActiveTabEmptyState()
    {
        if (pnlLoading.Visible) return;

        bool isEmpty = _activeTab switch
        {
            "media" => _sharedData.Media.Count == 0,
            "files" => _sharedData.Files.Count == 0 || flpFiles.Controls.Count == 0,
            "links" => _sharedData.Links.Count == 0,
            "voice" => _sharedData.VoiceMessages.Count == 0,
            _ => true
        };

        if (isEmpty)
        {
            (string icon, string title, string sub) = _activeTab switch
            {
                "media" => (IconHelper.Image, "Chưa có ảnh hoặc video", "Hình ảnh và video được chia sẻ trong trò chuyện sẽ hiển thị tại đây."),
                "files" => (IconHelper.File, "Chưa có tệp tin nào", "Tài liệu và tệp đính kèm được chia sẻ sẽ hiển thị tại đây."),
                "links" => (IconHelper.Link, "Chưa có liên kết nào", "Các đường dẫn (link) gửi trong tin nhắn sẽ hiển thị tại đây."),
                "voice" => (IconHelper.Mic, "Chưa có tin nhắn thoại", "Các bản ghi âm thoại trong cuộc trò chuyện sẽ hiển thị tại đây."),
                _ => (IconHelper.Folder, "Không có nội dung", "Chưa có nội dung chia sẻ.")
            };

            lblEmptyIcon.Text = icon;
            lblEmptyTitle.Text = title;
            lblEmptySubtitle.Text = sub;
            pnlEmptyState.Visible = true;
            pnlEmptyState.BringToFront();
        }
        else
        {
            pnlEmptyState.Visible = false;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Tab 1: Render Media (Ảnh & Video)
    // ─────────────────────────────────────────────────────────────────────────
    private void RenderMediaTab()
    {
        ClearThumbnails();
        flpMedia.SuspendLayout();
        flpMedia.Controls.Clear();

        foreach (var item in _sharedData.Media)
        {
            var card = CreateMediaCard(item);
            flpMedia.Controls.Add(card);
        }

        flpMedia.ResumeLayout();
    }

    private Control CreateMediaCard(SharedMediaItemDto item)
    {
        Panel card = new()
        {
            Size = new Size(100, 115),
            Margin = new Padding(3, 4, 3, 4),
            BackColor = Color.FromArgb(248, 250, 252),
            Cursor = Cursors.Hand
        };
        card.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, card.Width, card.Height, 8, 8));

        PictureBox pic = new()
        {
            Size = new Size(100, 84),
            Location = new Point(0, 0),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(241, 245, 249)
        };

        Label lblMeta = new()
        {
            Text = $"{VietnamTime.FromUtc(item.CreatedAt):HH:mm} • {item.SenderUsername}",
            Font = new Font("Segoe UI", 7F),
            ForeColor = Color.FromArgb(100, 116, 139),
            Location = new Point(2, 88),
            Size = new Size(96, 22),
            TextAlign = ContentAlignment.MiddleCenter
        };

        // Video badge
        if (item.IsVideo)
        {
            Label lblVid = new()
            {
                Text = "▶ VIDEO",
                Font = new Font("Segoe UI Semibold", 6.5F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(180, 15, 23, 42),
                Location = new Point(4, 62),
                Size = new Size(54, 18),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pic.Controls.Add(lblVid);
        }

        card.Controls.Add(lblMeta);
        card.Controls.Add(pic);

        // Tooltip
        ToolTip tt = new();
        tt.SetToolTip(card, $"{item.FileName}\r\nDung lượng: {FormatFileSize(item.FileSize)}\r\nNgười gửi: {item.SenderUsername}\r\nThời gian: {VietnamTime.FromUtc(item.CreatedAt):dd/MM/yyyy HH:mm}");
        tt.SetToolTip(pic, tt.GetToolTip(card));

        // Click handler: open viewer
        Action openAction = async () =>
        {
            var attDto = new AttachmentDto
            {
                Id = item.AttachmentId,
                OriginalFileName = item.FileName,
                FileSize = item.FileSize,
                RelativePath = item.RelativePath
            };

            string? localPath = await FileCacheService.Instance.EnsureCachedAsync(attDto, _session.ServerUrl, _apiClient);
            if (!string.IsNullOrEmpty(localPath) && File.Exists(localPath))
            {
                if (item.IsVideo)
                {
                    FileCacheService.Instance.OpenFileWithDefaultApp(localPath, out var err);
                    if (err != null) MessageBox.Show(err, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    using var viewer = new ImageViewerForm(localPath, item.FileName);
                    viewer.ShowDialog(FindForm());
                }
            }
            else
            {
                MessageBox.Show("Không thể tải tệp từ máy chủ LAN.", "Lỗi tải tệp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        };

        card.Click += (s, e) => openAction();
        pic.Click += (s, e) => openAction();

        // Context menu
        ContextMenuStrip menu = new();
        var menuOpen = menu.Items.Add(item.IsVideo ? "Mở video" : "Xem ảnh kích thước lớn");
        menuOpen.Click += (s, e) => openAction();

        var menuSave = menu.Items.Add("Lưu về máy...");
        menuSave.Click += async (s, e) =>
        {
            var attDto = new AttachmentDto
            {
                Id = item.AttachmentId,
                OriginalFileName = item.FileName,
                FileSize = item.FileSize,
                RelativePath = item.RelativePath
            };
            await SaveFileToDiskAsync(attDto);
        };

        card.ContextMenuStrip = menu;
        pic.ContextMenuStrip = menu;

        // Async load thumbnail
        LoadMediaThumbnailAsync(item, pic);

        return card;
    }

    private async void LoadMediaThumbnailAsync(SharedMediaItemDto item, PictureBox pic)
    {
        var attDto = new AttachmentDto
        {
            Id = item.AttachmentId,
            OriginalFileName = item.FileName,
            FileSize = item.FileSize,
            RelativePath = item.RelativePath
        };

        if (FileCacheService.Instance.IsCached(attDto, out string cachedPath))
        {
            SetThumbnailImage(pic, cachedPath);
            return;
        }

        // Asynchronously download to cache
        string? downloadedPath = await FileCacheService.Instance.EnsureCachedAsync(attDto, _session.ServerUrl, _apiClient);
        if (!string.IsNullOrEmpty(downloadedPath) && File.Exists(downloadedPath))
        {
            if (IsHandleCreated && !IsDisposed)
            {
                BeginInvoke(() => SetThumbnailImage(pic, downloadedPath));
            }
        }
    }

    private void SetThumbnailImage(PictureBox pic, string localPath)
    {
        try
        {
            using var fs = new FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var orig = Image.FromStream(fs);

            int thumbW = 140;
            int thumbH = 110;
            float ratio = Math.Min((float)thumbW / orig.Width, (float)thumbH / orig.Height);
            int finalW = Math.Max(40, (int)(orig.Width * ratio));
            int finalH = Math.Max(40, (int)(orig.Height * ratio));

            Bitmap thumb = new(orig, new Size(finalW, finalH));
            pic.Image?.Dispose();
            pic.Image = thumb;
        }
        catch
        {
            pic.BackColor = Color.FromArgb(241, 245, 249);
        }
    }

    private void ClearThumbnails()
    {
        foreach (Control c in flpMedia.Controls)
        {
            if (c is Panel p)
            {
                foreach (Control sub in p.Controls)
                {
                    if (sub is PictureBox pb && pb.Image != null)
                    {
                        pb.Image.Dispose();
                        pb.Image = null;
                    }
                }
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Tab 2: Render Files (Tệp tài liệu)
    // ─────────────────────────────────────────────────────────────────────────
    private void RenderFilesTab()
    {
        flpFiles.SuspendLayout();
        flpFiles.Controls.Clear();

        var query = _sharedData.Files.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(_fileSearchKeyword))
        {
            query = query.Where(f => f.FileName.Contains(_fileSearchKeyword, StringComparison.OrdinalIgnoreCase));
        }

        int itemWidth = flpFiles.ClientSize.Width - flpFiles.Padding.Horizontal - 6;
        if (itemWidth < 200) itemWidth = 310;

        foreach (var item in query)
        {
            var card = CreateFileCard(item, itemWidth);
            flpFiles.Controls.Add(card);
        }

        flpFiles.ResumeLayout();
    }

    private Control CreateFileCard(SharedFileItemDto item, int itemWidth)
    {
        Panel card = new()
        {
            Width = itemWidth,
            Height = 62,
            Margin = new Padding(0, 3, 0, 3),
            BackColor = Color.White,
            Padding = new Padding(6)
        };
        card.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, card.Width, card.Height, 8, 8));

        // Draw light card border
        card.Paint += (s, pe) =>
        {
            using var pen = new Pen(Color.FromArgb(226, 232, 240), 1);
            pe.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };

        // Icon badge
        Label lblIcon = new()
        {
            Text = IconHelper.GetFileTypeGlyph(item.FileName),
            Font = new Font("Segoe MDL2 Assets", 16F),
            ForeColor = IconHelper.GetFileTypeColor(item.FileName),
            Location = new Point(8, 12),
            Size = new Size(36, 36),
            TextAlign = ContentAlignment.MiddleCenter
        };

        // File info
        Label lblName = new()
        {
            Text = item.FileName,
            Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42),
            Location = new Point(48, 8),
            Size = new Size(card.Width - 165, 20),
            AutoEllipsis = true
        };

        Label lblMeta = new()
        {
            Text = $"{FormatFileSize(item.FileSize)} • {VietnamTime.FromUtc(item.CreatedAt):HH:mm} • {item.SenderUsername}",
            Font = new Font("Segoe UI", 7.5F),
            ForeColor = Color.FromArgb(100, 116, 139),
            Location = new Point(48, 30),
            Size = new Size(card.Width - 165, 18),
            AutoEllipsis = true
        };

        // Action Buttons
        Button btnOpen = new()
        {
            Text = "Mở",
            Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold),
            ForeColor = Color.FromArgb(37, 99, 235),
            BackColor = Color.FromArgb(239, 246, 255),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(46, 28),
            Location = new Point(card.Width - 106, 16),
            Cursor = Cursors.Hand
        };
        btnOpen.FlatAppearance.BorderSize = 0;
        btnOpen.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnOpen.Width, btnOpen.Height, 6, 6));

        Button btnSave = new()
        {
            Text = "Lưu",
            Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold),
            ForeColor = Color.FromArgb(71, 85, 105),
            BackColor = Color.FromArgb(241, 245, 249),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(46, 28),
            Location = new Point(card.Width - 54, 16),
            Cursor = Cursors.Hand
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnSave.Width, btnSave.Height, 6, 6));

        var attDto = new AttachmentDto
        {
            Id = item.AttachmentId,
            OriginalFileName = item.FileName,
            FileSize = item.FileSize,
            RelativePath = item.RelativePath
        };

        btnOpen.Click += async (s, e) =>
        {
            btnOpen.Enabled = false;
            btnOpen.Text = "...";
            try
            {
                string? localPath = await FileCacheService.Instance.EnsureCachedAsync(attDto, _session.ServerUrl, _apiClient);
                if (!string.IsNullOrEmpty(localPath) && File.Exists(localPath))
                {
                    if (!FileCacheService.Instance.OpenFileWithDefaultApp(localPath, out string? err))
                    {
                        MessageBox.Show(err ?? "Không thể mở tệp.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Không thể tải tệp tin từ máy chủ LAN.", "Lỗi tải tệp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            finally
            {
                btnOpen.Enabled = true;
                btnOpen.Text = "Mở";
            }
        };

        btnSave.Click += async (s, e) =>
        {
            await SaveFileToDiskAsync(attDto);
        };

        card.Controls.Add(lblIcon);
        card.Controls.Add(lblName);
        card.Controls.Add(lblMeta);
        card.Controls.Add(btnOpen);
        card.Controls.Add(btnSave);

        // Tooltip
        ToolTip tt = new();
        tt.SetToolTip(lblName, $"{item.FileName}\r\nDung lượng: {FormatFileSize(item.FileSize)}\r\nNgười gửi: {item.SenderUsername}\r\nThời gian: {VietnamTime.FromUtc(item.CreatedAt):dd/MM/yyyy HH:mm}");

        return card;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Tab 3: Render Links (Liên kết)
    // ─────────────────────────────────────────────────────────────────────────
    private void RenderLinksTab()
    {
        flpLinks.SuspendLayout();
        flpLinks.Controls.Clear();

        int itemWidth = flpLinks.ClientSize.Width - flpLinks.Padding.Horizontal - 6;
        if (itemWidth < 200) itemWidth = 310;

        foreach (var item in _sharedData.Links)
        {
            var card = CreateLinkCard(item, itemWidth);
            flpLinks.Controls.Add(card);
        }

        flpLinks.ResumeLayout();
    }

    private Control CreateLinkCard(SharedLinkItemDto item, int itemWidth)
    {
        Panel card = new()
        {
            Width = itemWidth,
            Height = 56,
            Margin = new Padding(0, 3, 0, 3),
            BackColor = Color.White,
            Padding = new Padding(6),
            Cursor = Cursors.Hand
        };
        card.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, card.Width, card.Height, 8, 8));

        card.Paint += (s, pe) =>
        {
            using var pen = new Pen(Color.FromArgb(226, 232, 240), 1);
            pe.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };

        Label lblIcon = new()
        {
            Text = IconHelper.Link,
            Font = new Font("Segoe MDL2 Assets", 12F),
            ForeColor = Color.FromArgb(59, 130, 246),
            Location = new Point(8, 12),
            Size = new Size(30, 30),
            TextAlign = ContentAlignment.MiddleCenter
        };

        Label lblUrl = new()
        {
            Text = item.Url,
            Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(37, 99, 235),
            Location = new Point(42, 8),
            Size = new Size(card.Width - 52, 20),
            AutoEllipsis = true,
            Cursor = Cursors.Hand
        };

        Label lblMeta = new()
        {
            Text = $"{VietnamTime.FromUtc(item.CreatedAt):HH:mm} • {item.SenderUsername}",
            Font = new Font("Segoe UI", 7.5F),
            ForeColor = Color.FromArgb(100, 116, 139),
            Location = new Point(42, 28),
            Size = new Size(card.Width - 52, 18),
            AutoEllipsis = true
        };

        Action openLinkAction = () =>
        {
            try
            {
                Process.Start(new ProcessStartInfo(item.Url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể mở liên kết: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        card.Click += (s, e) => openLinkAction();
        lblUrl.Click += (s, e) => openLinkAction();

        // Context menu
        ContextMenuStrip menu = new();
        var menuOpen = menu.Items.Add("Mở trong trình duyệt");
        menuOpen.Click += (s, e) => openLinkAction();

        var menuCopy = menu.Items.Add("Sao chép liên kết");
        menuCopy.Click += (s, e) =>
        {
            try
            {
                Clipboard.SetText(item.Url);
                MessageBox.Show("Đã sao chép liên kết vào bộ nhớ tạm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch { }
        };

        card.ContextMenuStrip = menu;
        lblUrl.ContextMenuStrip = menu;

        card.Controls.Add(lblIcon);
        card.Controls.Add(lblUrl);
        card.Controls.Add(lblMeta);

        return card;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Tab 4: Render Voice Messages (Tin nhắn thoại)
    // ─────────────────────────────────────────────────────────────────────────
    private void RenderVoiceTab()
    {
        _voiceControls.Clear();
        flpVoice.SuspendLayout();
        flpVoice.Controls.Clear();

        int itemWidth = flpVoice.ClientSize.Width - flpVoice.Padding.Horizontal - 6;
        if (itemWidth < 200) itemWidth = 310;

        foreach (var item in _sharedData.VoiceMessages)
        {
            var card = CreateVoiceCard(item, itemWidth);
            flpVoice.Controls.Add(card);
        }

        flpVoice.ResumeLayout();
    }

    private Control CreateVoiceCard(SharedVoiceItemDto item, int itemWidth)
    {
        Panel card = new()
        {
            Width = itemWidth,
            Height = 64,
            Margin = new Padding(0, 3, 0, 3),
            BackColor = Color.White,
            Padding = new Padding(6)
        };
        card.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, card.Width, card.Height, 8, 8));

        card.Paint += (s, pe) =>
        {
            using var pen = new Pen(Color.FromArgb(226, 232, 240), 1);
            pe.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };

        Button btnPlay = new()
        {
            Text = (VoicePlayerManager.Instance.CurrentAttachmentId == item.AttachmentId && VoicePlayerManager.Instance.IsPlaying) ? "⏸" : "▶",
            Font = new Font("Segoe UI Symbol", 14F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(37, 99, 235),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(40, 40),
            Location = new Point(6, 12),
            Cursor = Cursors.Hand,
            AccessibleName = "Phát tin nhắn thoại",
            AccessibleDescription = $"Phát tin nhắn thoại {item.FileName}."
        };
        SetVoicePlaybackButtonState(btnPlay, VoicePlayerManager.Instance.CurrentAttachmentId == item.AttachmentId && VoicePlayerManager.Instance.IsPlaying, item.FileName);
        btnPlay.FlatAppearance.BorderSize = 0;
        btnPlay.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnPlay.Width, btnPlay.Height, btnPlay.Width, btnPlay.Height));

        Panel pnlProgress = new()
        {
            Height = 6,
            Width = card.Width - 120,
            Location = new Point(50, 16),
            BackColor = Color.FromArgb(226, 232, 240)
        };
        pnlProgress.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlProgress.Width, pnlProgress.Height, 4, 4));

        Panel pnlBar = new()
        {
            Height = 6,
            Width = 0,
            Location = new Point(0, 0),
            BackColor = Color.FromArgb(37, 99, 235)
        };
        pnlProgress.Controls.Add(pnlBar);

        int dur = item.DurationSeconds;
        Label lblTime = new()
        {
            Text = $"{dur / 60:D2}:{dur % 60:D2}",
            Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold),
            ForeColor = Color.FromArgb(71, 85, 105),
            Location = new Point(card.Width - 62, 10),
            Size = new Size(52, 18),
            TextAlign = ContentAlignment.MiddleRight
        };

        Label lblMeta = new()
        {
            Text = $"{VietnamTime.FromUtc(item.CreatedAt):HH:mm} • {item.SenderUsername}",
            Font = new Font("Segoe UI", 7.5F),
            ForeColor = Color.FromArgb(100, 116, 139),
            Location = new Point(50, 32),
            Size = new Size(card.Width - 60, 18),
            AutoEllipsis = true
        };

        btnPlay.Click += async (s, e) =>
        {
            if (VoicePlayerManager.Instance.CurrentAttachmentId == item.AttachmentId && VoicePlayerManager.Instance.IsPlaying)
            {
                VoicePlayerManager.Instance.Pause(item.AttachmentId);
                return;
            }

            btnPlay.Enabled = false;
            try
            {
                var attDto = new AttachmentDto
                {
                    Id = item.AttachmentId,
                    OriginalFileName = item.FileName,
                    DurationSeconds = item.DurationSeconds
                };

                string? localWav = await FileCacheService.Instance.EnsureCachedAsync(attDto, _session.ServerUrl, _apiClient);
                if (!string.IsNullOrEmpty(localWav) && File.Exists(localWav))
                {
                    VoicePlayerManager.Instance.Play(item.AttachmentId, localWav);
                }
                else
                {
                    MessageBox.Show("Không thể tải tin nhắn thoại từ máy chủ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            finally
            {
                btnPlay.Enabled = true;
            }
        };

        card.Controls.Add(btnPlay);
        card.Controls.Add(pnlProgress);
        card.Controls.Add(lblTime);
        card.Controls.Add(lblMeta);

        _voiceControls[item.AttachmentId] = (btnPlay, pnlBar, lblTime);

        return card;
    }

    private void VoicePlayer_OnProgress(Guid attId, float progress, TimeSpan current, TimeSpan total)
    {
        if (_voiceControls.TryGetValue(attId, out var tuple) && IsHandleCreated)
        {
            try
            {
                BeginInvoke(() =>
                {
                    int parentWidth = tuple.pnlBar.Parent?.Width ?? 100;
                    tuple.pnlBar.Width = Math.Clamp((int)(parentWidth * progress), 0, parentWidth);
                    tuple.lblTime.Text = $"{current:mm\\:ss} / {total:mm\\:ss}";
                });
            }
            catch { }
        }
    }

    private void VoicePlayer_OnStateChanged(Guid attId, bool isPlaying)
    {
        if (_voiceControls.TryGetValue(attId, out var tuple) && IsHandleCreated)
        {
            try
            {
                BeginInvoke(() =>
                {
                    var item = _sharedData.VoiceMessages.FirstOrDefault(v => v.AttachmentId == attId);
                    SetVoicePlaybackButtonState(tuple.btnPlay, isPlaying, item?.FileName ?? "");
                });
            }
            catch { }
        }
    }

    private void VoicePlayer_OnStopped(Guid attId)
    {
        if (_voiceControls.TryGetValue(attId, out var tuple) && IsHandleCreated)
        {
            try
            {
                BeginInvoke(() =>
                {
                    var item = _sharedData.VoiceMessages.FirstOrDefault(v => v.AttachmentId == attId);
                    SetVoicePlaybackButtonState(tuple.btnPlay, isPlaying: false, item?.FileName ?? "");
                    tuple.pnlBar.Width = 0;
                    int dur = item?.DurationSeconds ?? 0;
                    tuple.lblTime.Text = $"{dur / 60:D2}:{dur % 60:D2}";
                });
            }
            catch { }
        }
    }

    private void SetVoicePlaybackButtonState(Button button, bool isPlaying, string fileName)
    {
        string action = isPlaying ? "Tạm dừng" : "Phát";
        IconHelper.SetButtonIcon(button, isPlaying ? IconHelper.Pause : IconHelper.Play, Color.White, 20, 14F);
        button.AccessibleName = $"{action} tin nhắn thoại";
        button.AccessibleDescription = string.IsNullOrWhiteSpace(fileName)
            ? $"{action} tin nhắn thoại."
            : $"{action} tin nhắn thoại {fileName}.";
        _toolTip.SetToolTip(button, $"{action} tin nhắn thoại");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────
    private void RenderAllTabs()
    {
        RenderMediaTab();
        RenderFilesTab();
        RenderLinksTab();
        RenderVoiceTab();
    }

    private async Task SaveFileToDiskAsync(AttachmentDto att)
    {
        using SaveFileDialog sfd = new()
        {
            FileName = att.OriginalFileName,
            Filter = $"Tệp (*{Path.GetExtension(att.OriginalFileName)})|*{Path.GetExtension(att.OriginalFileName)}|Tất cả tệp (*.*)|*.*",
            Title = "Lưu tệp tin về máy tính"
        };

        if (sfd.ShowDialog() == DialogResult.OK)
        {
            string? localPath = await FileCacheService.Instance.EnsureCachedAsync(att, _session.ServerUrl, _apiClient);
            if (!string.IsNullOrEmpty(localPath) && File.Exists(localPath))
            {
                try
                {
                    File.Copy(localPath, sfd.FileName, true);
                    MessageBox.Show("Đã lưu tệp tin thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Không thể sao chép tệp: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Không thể tải tệp tin từ máy chủ để lưu.", "Lỗi tải tệp", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes <= 0) return "0 B";
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F1} MB";
    }

    private static string GetFileIconEmoji(string fileName)
    {
        return IconHelper.GetFileTypeGlyph(fileName);
    }
}
