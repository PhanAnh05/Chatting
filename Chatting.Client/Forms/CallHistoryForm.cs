using Chatting.Client.Controls;
using Chatting.Client.Services;
using Chatting.Shared.Enums;

namespace Chatting.Client.Forms;

public partial class CallHistoryForm : Form
{
    private readonly CallHistoryControl _callHistoryControl;

    public event Action<Guid, Guid?, CallType>? OnCallbackRequested;
    public event Action<Guid>? OnOpenConversationRequested;

    /// <summary>
    /// Safe constructor. Does NOT initiate any calls.
    /// </summary>
    public CallHistoryForm(ApiClient apiClient, SessionManager session, SignalRService signalR)
    {
        InitializeComponent();

        _callHistoryControl = new CallHistoryControl(apiClient, session)
        {
            Dock = DockStyle.Fill
        };

        _callHistoryControl.OnCallbackRequested += (convId, receiverId, type) =>
        {
            OnCallbackRequested?.Invoke(convId, receiverId, type);
        };

        _callHistoryControl.OnOpenConversationRequested += (convId) =>
        {
            OnOpenConversationRequested?.Invoke(convId);
            Close();
        };

        Controls.Add(_callHistoryControl);

        Load += async (s, e) =>
        {
            await _callHistoryControl.LoadHistoryAsync();
        };
    }
}
