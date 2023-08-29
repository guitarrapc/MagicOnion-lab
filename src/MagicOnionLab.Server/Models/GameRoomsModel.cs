using System.Collections.Concurrent;
using UnityEngine;

namespace MagicOnionLab.Server.Models;

public class GameRoomsModel
{
    private readonly ConcurrentDictionary<string, int> _roomInfos;
    private readonly ConcurrentDictionary<string, List<MatchEntry>> _matchings;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> _matchCompletes;
    private readonly ConcurrentDictionary<string, List<UserUpdateEntry>> _userInfoupdates;

    public GameRoomsModel()
    {
        _roomInfos = new ConcurrentDictionary<string, int>();
        _matchings = new ConcurrentDictionary<string, List<MatchEntry>>();
        _matchCompletes = new ConcurrentDictionary<string, TaskCompletionSource<bool>>();
        _userInfoupdates = new ConcurrentDictionary<string, List<UserUpdateEntry>>();
    }

    /// <summary>
    /// Create room
    /// </summary>
    /// <param name="roomName"></param>
    /// <param name="capacity"></param>
    /// <returns></returns>
    public bool TryCreateRoom(string roomName, int capacity)
    {
        lock (roomName)
        {
            var roomInfos = _roomInfos.TryAdd(roomName, capacity);
            var matchComplete = _matchCompletes.TryAdd(roomName, new TaskCompletionSource<bool>());
            return roomInfos;
        }
    }

    /// <summary>
    /// Join room
    /// </summary>
    /// <param name="roomName"></param>
    /// <param name="userName"></param>
    /// <returns></returns>
    /// <exception cref="GameHubException"></exception>
    public bool TryJoinRoom(string roomName, string userName)
    {
        var userInfos = _userInfoupdates.GetOrAdd(roomName, new List<UserUpdateEntry>());

        lock (userInfos)
        {
            // already room full-filled.
            if (_roomInfos.TryGetValue(roomName, out var capacity) && userInfos.Count > capacity)
            {
                // Trying to join room but room capacity full filled.
                return false;
            }

            if (!userInfos.Exists(x => x.UserName == userName))
            {
                userInfos.Add(new UserUpdateEntry(userName));
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Update Userinfo
    /// </summary>
    /// <param name="roomName"></param>
    /// <param name="userName"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool TryUpdateUserverInfo(string roomName, string userName, Vector3 position)
    {
        if (_userInfoupdates.TryGetValue(roomName, out var current))
        {
            lock (current)
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

    /// <summary>
    /// Leave Room
    /// </summary>
    /// <param name="roomName"></param>
    /// <returns></returns>
    public bool TryLeaveRoom(string roomName)
    {
        return _userInfoupdates.TryRemove(roomName, out var _);
    }

    /// <summary>
    /// Ready matching for room
    /// </summary>
    /// <param name="roomName"></param>
    /// <param name="userName"></param>
    /// <param name="ready"></param>
    public void ReadyMatch(string roomName, string userName, bool ready)
    {
        var matchings = _matchings.GetOrAdd(roomName, new List<MatchEntry>());
        lock (matchings)
        {
            if (!matchings.Exists(x => x.UserName.Equals(userName, StringComparison.Ordinal)))
            {
                matchings.Add(new MatchEntry
                {
                    Ready = ready,
                    UserName = userName,
                });
            }
        }
    }

    /// <summary>
    /// Wait until matching complete.
    /// </summary>
    /// <param name="roomName"></param>
    /// <returns></returns>
    public Task WaitMatchingCompletedAsync(string roomName)
    {
        if (!_roomInfos.TryGetValue(roomName, out var capacity))
        {
            throw new GameHubException($"Room not initialized. {roomName}");
        }
        if (!_matchings.TryGetValue(roomName, out var matching))
        {
            throw new GameHubException($"Matching not initialized. {roomName}");
        }

        var task = _matchCompletes.TryGetValue(roomName, out var matchTcs) ? matchTcs.Task : throw new ArgumentNullException(nameof(matchTcs));
        if (_userInfoupdates.TryGetValue(roomName, out var users))
        {
            if (users.Count == capacity && matching.Count == capacity)
            {
                var result = matching.All(x => x.Ready);
                if (result && _matchCompletes.TryGetValue(roomName, out var tcs))
                {
                    // complete match when both member and ready fullfilled.
                    tcs.TrySetResult(true);
                }
            }
        }

        // wait
        return task;
    }

    /// <summary>
    /// Clear Matching info. Only first call return true, others return false.
    /// </summary>
    /// <param name="roomName"></param>
    /// <returns></returns>
    public bool TryClearMatchInfo(string roomName)
    {
        var matching = _matchings.TryRemove(roomName, out var _);
        var matchComplete = _matchCompletes.TryRemove(roomName, out var _);
        return matching;
    }
}
