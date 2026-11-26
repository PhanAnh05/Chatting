namespace Chatting.Shared.Enums;

public enum UserStatus
{
    Offline = 0,
    Online = 1,
    Busy = 2
}

public enum FriendRequestStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2,
    Canceled = 3
}

public enum GroupRole
{
    Member = 0,
    Leader = 1
}

public enum ConversationType
{
    Direct = 0,
    Group = 1
}

public enum MessageType
{
    Text = 0,
    Image = 1,
    Video = 2,
    Document = 3,
    Voice = 4,
    System = 5
}

public enum MessageStatus
{
    Sent = 0,
    Delivered = 1,
    Seen = 2,
    Edited = 3,
    Revoked = 4,
    Deleted = 5
}

public enum CallType
{
    Audio = 0,
    Video = 1
}

public enum CallStatus
{
    Ringing = 0,
    Ongoing = 1,
    Ended = 2,
    Rejected = 3,
    Missed = 4,
    Disconnected = 5
}
