using System.Collections.Concurrent;
using UnityEngine;

namespace MagicOnionLab.Server.Models;

public class UserPosition
{
    public Vector3 PositionCurrent => _positionCurrent;
    private Vector3 _positionCurrent;
    public ConcurrentQueue<Vector3> PositionHistory { get; }
    public string UserName { get; }

    private readonly int _historyCount;

    public UserPosition(string userName, int historyCount = 50)
    {
        _historyCount = historyCount;
        UserName = userName;
        PositionHistory = new ConcurrentQueue<Vector3>();
    }

    public void UpdatePosition(Vector3 position)
    {
        _positionCurrent = position;
        PositionHistory.Enqueue(position);

        // drop overflowed history
        if (PositionHistory.Count > _historyCount)
        {
            PositionHistory.TryDequeue(out var _);
        }
    }
}
