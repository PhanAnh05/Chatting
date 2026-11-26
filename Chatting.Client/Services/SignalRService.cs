using Chatting.Shared.DTOs;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Chatting.Client.Services;

/// <summary>
/// Manages the SignalR HubConnection lifecycle and exposes typed event callbacks
/// that UI forms can subscribe to.
/// </summary>
public class SignalRService : IAsyncDisposable
{
    private readonly SessionManager _session;
    private HubConnection? _connection;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    // ── Events raised on the UI thread via SynchronizationContext ────────────
    public event Action<MessageDto>? OnReceiveMessage;
    public event Action<Guid, Guid, bool>? OnUserTyping;       // (convId, userId, isTyping)
    public event Action<Guid, Guid, DateTime>? OnMessagesRead; // (convId, readByUserId, readAt)
    public event Action<string>? OnFriendStatusChanged;         // raw JSON
    public event Action<FriendRequestDto>? OnNewFriendRequest;
    public event Action<FriendRequestDto>? OnFriendRequestAccepted;
    public event Action<Guid>? OnFriendRequestRejected;
    public event Action<Guid>? OnFriendRequestCanceled;
    public event Action<Guid>? OnFriendRemoved;
    public event Action<Guid>? OnNewConversationCreated;
    public event Action<CallInfoDto>? OnIncomingCall;
    public event Action<CallInfoDto>? OnCallStarted;
    public event Action<CallInfoDto>? OnCallAccepted;
    public event Action<CallEndedDto>? OnCallEnded;
    public event Action<MessageEditedDto>? OnMessageEdited;
    public event Action<MessagePinChangedDto>? OnMessagePinUpdated;
    public event Action<MessageStateChangedDto>? OnMessageRevoked;
    public event Action<MessageReactionsChangedDto>? OnMessageReactionsUpdated;
    public event Action<MessageStateChangedDto>? OnMessageHiddenForMe;
    public event Action<WebRtcSignalDto>? OnReceiveWebRtcSignal;
    public event Action<List<GroupCallParticipantDto>>? OnGroupCallParticipants;
    public event Action<GroupCallParticipantDto>? OnGroupCallParticipantJoined;
    public event Action<GroupCallParticipantDto>? OnGroupCallParticipantLeft;
    public event Action<UserProfileDto>? OnUserProfileUpdated;
    public event Action<Guid, Guid, string?, MessageDto>? OnNicknameUpdated;
    public event Action<string>? OnGroupUpdated;
    public event Action<GroupMembersAddedDto>? OnMembersAdded;
    public event Action<GroupMemberRemovedDto>? OnMemberRemoved;
    public event Action<GroupMemberRemovedDto>? OnYouWereRemovedFromGroup;
    public event Action<GroupMemberChangedDto>? OnMemberLeft;
    public event Action<string>? OnMemberRoleChanged;
    public event Action? OnConnected;
    public event Action? OnDisconnected;
    public event Action<Exception?>? OnReconnecting;
    public event Action<string?>? OnReconnected;

    private readonly SynchronizationContext? _uiContext;

    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    public SignalRService(SessionManager session)
    {
        _session = session;
        _uiContext = SynchronizationContext.Current;
    }

    // ─────────────────────────────────────────────────────────────────────────
    public async Task ConnectAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }

        string hubUrl = $"{_session.ServerUrl}/hubs/chat";

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, opts =>
            {
                opts.AccessTokenProvider = () => Task.FromResult<string?>(_session.AccessToken);
            })
            .WithAutomaticReconnect([TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10)])
            .Build();

        RegisterHandlers();

        _connection.Reconnecting += ex =>
        {
            Post(() => OnReconnecting?.Invoke(ex));
            return Task.CompletedTask;
        };
        _connection.Reconnected += connId =>
        {
            Post(() => OnReconnected?.Invoke(connId));
            return Task.CompletedTask;
        };
        _connection.Closed += _ =>
        {
            Post(() => OnDisconnected?.Invoke());
            return Task.CompletedTask;
        };

        await _connection.StartAsync();
        Post(() => OnConnected?.Invoke());
    }

    public async Task DisconnectAsync()
    {
        if (_connection is not null)
            await _connection.StopAsync();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Hub method invocations
    // ─────────────────────────────────────────────────────────────────────────
    public Task SendMessageAsync(SendMessageDto dto) =>
        InvokeAsync("SendMessage", dto);

    public Task SendTypingAsync(Guid conversationId, bool isTyping) =>
        InvokeAsync("SendTyping", conversationId, isTyping);

    public Task MarkConversationReadAsync(Guid conversationId) =>
        InvokeAsync("MarkConversationRead", conversationId);

    public Task StartCallAsync(StartCallRequestDto dto) =>
        InvokeAsync("StartCall", dto);

    public async Task<bool?> AcceptCallAsync(Guid callId)
    {
        if (_connection?.State != HubConnectionState.Connected)
            return null;

        return await _connection.InvokeCoreAsync<bool>("AcceptCall", [callId]);
    }

    public Task RejectCallAsync(Guid callId) =>
        InvokeAsync("RejectCall", callId);

    public Task EndCallAsync(Guid callId) =>
        InvokeAsync("EndCall", callId);

    public Task SendWebRtcSignalAsync(WebRtcSignalDto dto) =>
        InvokeAsync("SendWebRtcSignal", dto);

    public Task JoinConversationAsync(Guid conversationId) =>
        InvokeAsync("JoinConversation", conversationId);

    // ─────────────────────────────────────────────────────────────────────────
    // Register server → client event handlers
    // ─────────────────────────────────────────────────────────────────────────
    private void RegisterHandlers()
    {
        if (_connection is null) return;

        _connection.On<MessageDto>("ReceiveMessage", msg =>
            Post(() => OnReceiveMessage?.Invoke(msg)));

        _connection.On<object>("UserTyping", raw =>
        {
            try
            {
                string json = JsonSerializer.Serialize(raw);
                using JsonDocument doc = JsonDocument.Parse(json);
                Guid convId = doc.RootElement.GetProperty("conversationId").GetGuid();
                Guid userId = doc.RootElement.GetProperty("userId").GetGuid();
                bool typing = doc.RootElement.GetProperty("isTyping").GetBoolean();
                Post(() => OnUserTyping?.Invoke(convId, userId, typing));
            }
            catch { }
        });

        _connection.On<object>("MessagesRead", raw =>
        {
            try
            {
                string json = JsonSerializer.Serialize(raw);
                using JsonDocument doc = JsonDocument.Parse(json);
                Guid convId = doc.RootElement.GetProperty("conversationId").GetGuid();
                Guid userId = doc.RootElement.GetProperty("readByUserId").GetGuid();
                DateTime readAt = doc.RootElement.GetProperty("readAt").GetDateTime();
                Post(() => OnMessagesRead?.Invoke(convId, userId, readAt));
            }
            catch { }
        });

        _connection.On<object>("FriendStatusChanged", raw =>
            Post(() => OnFriendStatusChanged?.Invoke(JsonSerializer.Serialize(raw))));

        _connection.On<FriendRequestDto>("NewFriendRequest", dto =>
            Post(() => OnNewFriendRequest?.Invoke(dto)));

        _connection.On<FriendRequestDto>("FriendRequestAccepted", dto =>
            Post(() => OnFriendRequestAccepted?.Invoke(dto)));

        _connection.On<object>("FriendRequestRejected", raw =>
        {
            try
            {
                string json = JsonSerializer.Serialize(raw);
                Guid id = JsonDocument.Parse(json).RootElement.GetProperty("requestId").GetGuid();
                Post(() => OnFriendRequestRejected?.Invoke(id));
            }
            catch { }
        });

        _connection.On<object>("FriendRequestCanceled", raw =>
        {
            try
            {
                string json = JsonSerializer.Serialize(raw);
                Guid id = JsonDocument.Parse(json).RootElement.GetProperty("requestId").GetGuid();
                Post(() => OnFriendRequestCanceled?.Invoke(id));
            }
            catch { }
        });

        _connection.On<object>("FriendRemoved", raw =>
        {
            try
            {
                string json = JsonSerializer.Serialize(raw);
                Guid uid = JsonDocument.Parse(json).RootElement.GetProperty("userId").GetGuid();
                Post(() => OnFriendRemoved?.Invoke(uid));
            }
            catch { }
        });

        _connection.On<object>("NewConversationCreated", raw =>
        {
            try
            {
                string json = JsonSerializer.Serialize(raw);
                Guid cid = JsonDocument.Parse(json).RootElement.GetProperty("conversationId").GetGuid();
                Post(() => OnNewConversationCreated?.Invoke(cid));
            }
            catch { }
        });

        _connection.On<CallInfoDto>("IncomingCall", dto =>
            Post(() => OnIncomingCall?.Invoke(dto)));

        _connection.On<CallInfoDto>("CallStarted", dto =>
            Post(() => OnCallStarted?.Invoke(dto)));

        _connection.On<CallInfoDto>("CallAccepted", dto =>
            Post(() => OnCallAccepted?.Invoke(dto)));

        _connection.On<CallEndedDto>("CallEnded", dto =>
            Post(() => OnCallEnded?.Invoke(dto)));

        _connection.On<WebRtcSignalDto>("ReceiveWebRtcSignal", dto =>
            Post(() => OnReceiveWebRtcSignal?.Invoke(dto)));

        _connection.On<List<GroupCallParticipantDto>>("GroupCallParticipants", participants =>
            Post(() => OnGroupCallParticipants?.Invoke(participants)));

        _connection.On<GroupCallParticipantDto>("GroupCallParticipantJoined", participant =>
            Post(() => OnGroupCallParticipantJoined?.Invoke(participant)));

        _connection.On<GroupCallParticipantDto>("GroupCallParticipantLeft", participant =>
            Post(() => OnGroupCallParticipantLeft?.Invoke(participant)));

        _connection.On<UserProfileDto>("UserProfileUpdated", dto =>
            Post(() => OnUserProfileUpdated?.Invoke(dto)));

        _connection.On<Guid, Guid, string?, MessageDto>("NicknameUpdated", (conversationId, targetUserId, nickname, message) =>
            Post(() => OnNicknameUpdated?.Invoke(conversationId, targetUserId, nickname, message)));

        _connection.On<object>("GroupUpdated", raw =>
            Post(() => OnGroupUpdated?.Invoke(JsonSerializer.Serialize(raw))));

        _connection.On<GroupMembersAddedDto>("MembersAdded", dto =>
            Post(() => OnMembersAdded?.Invoke(dto)));

        _connection.On<GroupMemberRemovedDto>("MemberRemoved", dto =>
            Post(() => OnMemberRemoved?.Invoke(dto)));

        _connection.On<GroupMemberRemovedDto>("YouWereRemovedFromGroup", dto =>
            Post(() => OnYouWereRemovedFromGroup?.Invoke(dto)));

        _connection.On<GroupMemberChangedDto>("MemberLeft", dto =>
            Post(() => OnMemberLeft?.Invoke(dto)));

        _connection.On<object>("MemberRoleChanged", raw =>
            Post(() => OnMemberRoleChanged?.Invoke(JsonSerializer.Serialize(raw))));

        _connection.On<MessageEditedDto>("MessageEdited", dto =>
            Post(() => OnMessageEdited?.Invoke(dto)));

        _connection.On<MessagePinChangedDto>("MessagePinUpdated", dto =>
            Post(() => OnMessagePinUpdated?.Invoke(dto)));

        _connection.On<MessageStateChangedDto>("MessageRevoked", dto =>
            Post(() => OnMessageRevoked?.Invoke(dto)));

        _connection.On<MessageReactionsChangedDto>("MessageReactionsUpdated", dto =>
            Post(() => OnMessageReactionsUpdated?.Invoke(dto)));

        _connection.On<MessageStateChangedDto>("MessageHiddenForMe", dto =>
            Post(() => OnMessageHiddenForMe?.Invoke(dto)));
    }

    private async Task InvokeAsync(string method, params object?[] args)
    {
        if (_connection?.State == HubConnectionState.Connected)
            await _connection.InvokeCoreAsync(method, args);
    }

    private void Post(Action action)
    {
        if (_uiContext is not null)
            _uiContext.Post(_ => action(), null);
        else
            action();
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}
