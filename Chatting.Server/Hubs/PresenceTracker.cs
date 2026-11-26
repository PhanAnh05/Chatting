using System.Collections.Concurrent;

namespace Chatting.Server.Hubs;

/// <summary>
/// Thread-safe in-memory tracker mapping UserId -> set of SignalR ConnectionIds.
/// </summary>
public class PresenceTracker
{
    private readonly ConcurrentDictionary<Guid, HashSet<string>> _connections = new();
    private readonly object _lock = new();

    public void UserConnected(Guid userId, string connectionId)
    {
        lock (_lock)
        {
            if (!_connections.TryGetValue(userId, out HashSet<string>? conns))
            {
                conns = [];
                _connections[userId] = conns;
            }
            conns.Add(connectionId);
        }
    }

    public bool UserDisconnected(Guid userId, string connectionId)
    {
        lock (_lock)
        {
            if (!_connections.TryGetValue(userId, out HashSet<string>? conns))
                return false;
            conns.Remove(connectionId);
            if (conns.Count == 0)
            {
                _connections.TryRemove(userId, out _);
                return true; // user fully offline
            }
            return false;
        }
    }

    public bool IsOnline(Guid userId)
        => _connections.ContainsKey(userId) && _connections[userId].Count > 0;

    public IReadOnlyCollection<Guid> GetOnlineUsers()
        => (IReadOnlyCollection<Guid>)_connections.Keys.ToList().AsReadOnly();

    public IReadOnlyCollection<string> GetConnectionIds(Guid userId)
    {
        if (_connections.TryGetValue(userId, out HashSet<string>? conns))
            return conns.ToList().AsReadOnly();
        return Array.Empty<string>();
    }
}
