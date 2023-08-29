using System.Collections.Concurrent;
using UnityEngine;

namespace MagicOnionLab.Server.Models;

public class GameRoomsModel
{
    private readonly ConcurrentDictionary<string, int> _roomInfos;
    private readonly ConcurrentDictionary<string, List<MatchEntry>> _matchings;
    private readonly ConcurrentDictionary<string, List<UserUpdateEntry>> _userInfoupdates;

    public GameRoomsModel()
    {
        _roomInfos = new ConcurrentDictionary<string, int>();
        _userInfoupdates = new ConcurrentDictionary<string, List<UserUpdateEntry>>();
        _matchings = new ConcurrentDictionary<string, List<MatchEntry>>();
    }

    public bool TryCreate(string roomName, int capacity)
    {
        return _roomInfos.TryAdd(roomName, capacity) && _userInfoupdates.TryAdd(roomName, new List<UserUpdateEntry>());
    }

    public bool TryJoinRoom(string roomName, string userName)
    {
        if (_roomInfos.TryGetValue(roomName, out var capacity) && _userInfoupdates.TryGetValue(roomName, out var current))
        {
            if (current.Count > capacity)
            {
                throw new GameHubException($"Trying to join room but room capacity full filled.");
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
                current.Add(new UserUpdateEntry(userName));
                return true;
            }
        }
        return _userInfoupdates.TryAdd(roomName, new List<UserUpdateEntry> { new UserUpdateEntry(userName) });
    }

    public bool TryUpdateUserverInfo(string roomName, string userName, Vector3 position)
    {
        if (_userInfoupdates.TryGetValue(roomName, out var current))
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

    public bool TryLeaveRoom(string roomName)
    {
        return _userInfoupdates.TryRemove(roomName, out var _);
    }

    public bool TryReadyMatch(string roomName, string userName)
    {
        if (_matchings.TryGetValue(roomName, out var current))
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
            _matchings.TryAdd(roomName, new List<MatchEntry>
            {
                new MatchEntry
                {
                    UserName = userName,
                    Ready = true,
                }
            });
            return true;
        }
    }

    public bool IsMatchComplete(string roomName)
    {
        if (_roomInfos.TryGetValue(roomName, out var capacity) && _matchings.TryGetValue(roomName, out var current))
        {
            if (_userInfoupdates.TryGetValue(roomName, out var positions) && positions.Count == capacity)
            {
                return current.All(x => x.Ready);
            }
            return false;
        }
        return false;
    }

    public void ClearMatchInfo(string roomName)
    {
        _matchings.TryRemove(roomName, out var _);
    }
}
