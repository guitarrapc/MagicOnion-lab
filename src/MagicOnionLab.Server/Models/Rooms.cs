using System.Collections.Concurrent;
using UnityEngine;

namespace MagicOnionLab.Server.Models;

public class Rooms
{
    private readonly ConcurrentDictionary<string, int> _roomInfos;
    private readonly ConcurrentDictionary<string, List<UserPosition>> _userPositions;
    private readonly ConcurrentDictionary<string, List<UserMatch>> _userMatches;

    public Rooms()
    {
        _roomInfos = new ConcurrentDictionary<string, int>();
        _userPositions = new ConcurrentDictionary<string, List<UserPosition>>();
        _userMatches = new ConcurrentDictionary<string, List<UserMatch>>();
    }

    public bool TryCreate(string roomName, int capacity)
    {
        return _roomInfos.TryAdd(roomName, capacity) && _userPositions.TryAdd(roomName, new List<UserPosition>());
    }

    public bool TryJoin(string roomName, string userName)
    {
        if (_roomInfos.TryGetValue(roomName, out var capacity) && _userPositions.TryGetValue(roomName, out var current))
        {
            if (current.Count > capacity)
            {
                throw new PositionRoomException($"Trying to join room but room capacity full filled.");
            }
            lock (roomName)
            {
                for (var i = 0; i < current.Count; i++)
                {
                    if (current[i].UserName.Equals(userName, StringComparison.Ordinal))
                    {
                        return false;
                    }
                }
                current.Add(new UserPosition(userName));
                return true;
            }
        }
        return _userPositions.TryAdd(roomName, new List<UserPosition> { new UserPosition(userName) });
    }

    public bool TryUpdate(string roomName, string userName, Vector3 position)
    {
        if (_userPositions.TryGetValue(roomName, out var current))
        {
            lock (roomName)
            {
                for (var i = 0; i < current.Count; i++)
                {
                    if (current[i].UserName.Equals(userName, StringComparison.Ordinal))
                    {
                        // update
                        current[i].UpdatePosition(position);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public bool TryLeave(string roomName)
    {
        return _userPositions.TryRemove(roomName, out var _);
    }

    public bool TryReady(string roomName, string userName)
    {
        if (_userMatches.TryGetValue(roomName, out var current))
        {
            lock (roomName)
            {
                for (var i = 0; i < current.Count; i++)
                {
                    if (current[i].UserName.Equals(userName, StringComparison.Ordinal))
                    {
                        current[i].Ready = true;
                        return true;
                    }
                }
                return false;
            }
        }
        else
        {
            _userMatches.TryAdd(roomName, new List<UserMatch>
            {
                new UserMatch
                {
                    UserName = userName,
                    Ready = true,
                }
            });
            return true;
        }
    }

    public bool IsCompleteReady(string roomName)
    {
        if (_roomInfos.TryGetValue(roomName, out var capacity) && _userMatches.TryGetValue(roomName, out var current))
        {
            if (_userPositions.TryGetValue(roomName, out var positions) && positions.Count == capacity)
            {
                return current.All(x => x.Ready);
            }
            return false;
        }
        return false;
    }
}
