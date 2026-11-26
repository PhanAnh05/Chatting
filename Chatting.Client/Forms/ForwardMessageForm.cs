using Chatting.Shared.DTOs;
using Chatting.Shared.Enums;

namespace Chatting.Client.Forms;

public sealed class ForwardMessageForm : Form
{
    private sealed record ConversationChoice(Guid Id, string Title, ConversationType Type)
    {
        public override string ToString() => $"{(Type == ConversationType.Group ? "[Nhóm]" : "[Cá nhân]")} {Title}";
    }

    private readonly List<ConversationChoice> _allChoices;
    private readonly HashSet<Guid> _selectedIds = [];
    private readonly TextBox _search;
    private readonly CheckedListBox _conversationList;
    private readonly Button _forwardButton;
    private bool _renderingChoices;

    public List<Guid> SelectedConversationIds { get; private set; } = [];

    public ForwardMessageForm(IEnumerable<ConversationDto> conversations, bool darkTheme)
    {
        _allChoices = conversations
            .OrderBy(c => c.Title, StringComparer.CurrentCultureIgnoreCase)
            .Select(c => new ConversationChoice(c.Id,
                string.IsNullOrWhiteSpace(c.Title) ? "Cuộc trò chuyện" : c.Title, c.Type))
            .ToList();

        Color background = darkTheme ? Color.FromArgb(15, 23, 42) : Color.FromArgb(248, 250, 252);
        Color surface = darkTheme ? Color.FromArgb(30, 41, 59) : Color.White;
        Color text = darkTheme ? Color.FromArgb(226, 232, 240) : Color.FromArgb(15, 23, 42);
        Color muted = darkTheme ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139);

        Text = "Chuyển tiếp tin nhắn - Chatting";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(500, 560);
        BackColor = background;
        Font = new Font("Segoe UI", 9.5F);

        Label title = new()
        {
            AutoSize = false,
            Text = "Chuyển tiếp tin nhắn",
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = text,
            Location = new Point(24, 20),
            Size = new Size(450, 32)
        };
        Label description = new()
        {
            AutoSize = false,
            Text = "Chọn một hoặc nhiều cuộc trò chuyện để gửi tin nhắn này.",
            ForeColor = muted,
            Location = new Point(24, 58),
            Size = new Size(450, 24)
        };
        _search = new TextBox
        {
            PlaceholderText = "Tìm cuộc trò chuyện...",
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = surface,
            ForeColor = text,
            Location = new Point(24, 95),
            Size = new Size(452, 34)
        };
        _conversationList = new CheckedListBox
        {
            CheckOnClick = true,
            IntegralHeight = false,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = surface,
            ForeColor = text,
            Location = new Point(24, 140),
            Size = new Size(452, 340),
            Font = new Font("Segoe UI", 10F)
        };
        _forwardButton = new Button
        {
            Text = "Chuyển tiếp",
            DialogResult = DialogResult.None,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(37, 99, 235),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Location = new Point(338, 500),
            Size = new Size(138, 38)
        };
        _forwardButton.FlatAppearance.BorderSize = 0;
        Button cancelButton = new()
        {
            Text = "Hủy",
            DialogResult = DialogResult.Cancel,
            FlatStyle = FlatStyle.Flat,
            BackColor = surface,
            ForeColor = text,
            Cursor = Cursors.Hand,
            Location = new Point(238, 500),
            Size = new Size(90, 38)
        };

        Controls.AddRange([title, description, _search, _conversationList, cancelButton, _forwardButton]);
        AcceptButton = _forwardButton;
        CancelButton = cancelButton;
        _search.TextChanged += (_, _) => RenderConversationChoices();
        _conversationList.ItemCheck += (_, e) =>
        {
            if (_renderingChoices) return;
            if (e.Index < 0 || e.Index >= _conversationList.Items.Count) return;
            ConversationChoice choice = (ConversationChoice)_conversationList.Items[e.Index]!;
            if (e.NewValue == CheckState.Checked) _selectedIds.Add(choice.Id);
            else _selectedIds.Remove(choice.Id);
        };
        _forwardButton.Click += ForwardButton_Click;
        RenderConversationChoices();
    }

    private void RenderConversationChoices()
    {
        string keyword = _search.Text.Trim();
        _renderingChoices = true;
        _conversationList.BeginUpdate();
        try
        {
            _conversationList.Items.Clear();
            foreach (ConversationChoice choice in _allChoices.Where(choice =>
                         string.IsNullOrWhiteSpace(keyword) || choice.Title.Contains(keyword, StringComparison.CurrentCultureIgnoreCase)))
            {
                int index = _conversationList.Items.Add(choice);
                if (_selectedIds.Contains(choice.Id)) _conversationList.SetItemChecked(index, true);
            }
        }
        finally
        {
            _conversationList.EndUpdate();
            _renderingChoices = false;
        }
    }

    private void ForwardButton_Click(object? sender, EventArgs e)
    {
        SelectedConversationIds = _selectedIds.ToList();
        if (SelectedConversationIds.Count == 0)
        {
            MessageBox.Show(this, "Hãy chọn ít nhất một cuộc trò chuyện.", "Chuyển tiếp",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }
}
