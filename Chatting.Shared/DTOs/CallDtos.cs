using Chatting.Shared.Enums;

namespace Chatting.Shared.DTOs;

public class StartCallRequestDto
{
    public Guid ConversationId { get; set; }
    public Guid? ReceiverId { get; set; }
    public CallType CallType { get; set; }
}

public class CallInfoDto
{
    public Guid CallId { get; set; }
    public Guid ConversationId { get; set; }
    public Guid CallerId { get; set; }
    public string CallerUsername { get; set; } = string.Empty;
    public string? CallerDisplayName { get; set; }
    public string? CallerAvatarUrl { get; set; }
    public Guid? ReceiverId { get; set; }
    public string? ReceiverUsername { get; set; }
    public string? ReceiverDisplayName { get; set; }
    public string? ReceiverAvatarUrl { get; set; }
    public CallType CallType { get; set; }
    public CallStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? ConnectedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int? DurationSeconds { get; set; }
}

public class WebRtcSignalDto
{
    public Guid CallId { get; set; }
    public Guid SenderUserId { get; set; }
    public Guid TargetUserId { get; set; }
    public string SignalType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
}

public class GroupCallParticipantDto
{
    public Guid CallId { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
}

public class CallEndedDto
{
    public Guid CallId { get; set; }
    public CallStatus FinalStatus { get; set; }
    public int? DurationSeconds { get; set; }
}
