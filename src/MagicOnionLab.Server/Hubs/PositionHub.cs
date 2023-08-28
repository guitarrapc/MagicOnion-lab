#pragma warning disable CS8644 // Type does not implement interface member. Nullability of reference types in interface implemented by the base type doesn't match.
using MagicOnion.Server.Hubs;
using MagicOnionLab.Server.Models;
using MagicOnionLab.Shared.Hubs;
using MagicOnionLab.Shared.Mpos;

namespace MagicOnionLab.Server.Hubs;

public class PositionHub : StreamingHubBase<IPositionHub, IPositionHubReceiver>, IPositionHub
{
    private readonly ILogger<PositionHub> _logger;
    private IGroup? _room;
    private readonly Rooms _rooms;
    private string? _roomName;
    private string? _userName;

    public PositionHub(Rooms rooms, ILogger<PositionHub> logger)
    {
        _logger = logger;
        _rooms = rooms;
    }

    public async ValueTask CreateRoomAsync(PositionRoomCreateRequest request)
    {
        _room = await Group.AddAsync(request.RoomName);
        _roomName = request.RoomName;

        if (_rooms.TryCreate(request.RoomName, request.Capacity))
        {
            this.Broadcast(_room).OnCreate(request.RoomName);
        }
    }

    public async ValueTask JoinAsync(PositionRoomJoinRequest request)
    {
        ArgumentNullException.ThrowIfNull(_room);

        _userName = request.UserName;
        _rooms.TryJoin(request.RoomName, request.UserName);
        this.Broadcast(_room).OnJoin(request.UserName);
    }

    public async ValueTask ReadyAsync()
    {
        ArgumentNullException.ThrowIfNull(_room);
        ArgumentNullException.ThrowIfNullOrEmpty(_roomName);
        ArgumentNullException.ThrowIfNullOrEmpty(_userName);
        _rooms.TryReady(_roomName, _userName);
        if (_rooms.IsCompleteReady(_roomName))
        {
            this.Broadcast(_room).OnMatch();
        }
    }

    public async ValueTask LeaveAsync()
    {
        ArgumentNullException.ThrowIfNull(_room);
        await _room.RemoveAsync(Context);
        Broadcast(_room).OnLeave(_userName);
    }

    public ValueTask UpdatePosition(PositionRoomUpdateRequest request)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(_roomName);
        ArgumentNullException.ThrowIfNullOrEmpty(_userName);
        ArgumentNullException.ThrowIfNull(_room);

        _logger.LogInformation($"{nameof(UpdatePosition)}: {_userName} => ({request.Position.x},{request.Position.y},{request.Position.z})");
        _rooms.TryUpdate(_roomName, _userName, request.Position);
        Broadcast(_room).OnUpdatePosition(new PositionRoomUpdateResponse
        {
            UserName = _userName,
            Position = request.Position,
        });
        return ValueTask.CompletedTask;
    }

    protected override ValueTask OnConnecting()
    {
        _logger.LogInformation($"Client connected {Context.ContextId}");
        return CompletedTask;
    }

    protected override ValueTask OnDisconnected()
    {
        return CompletedTask;
    }
}
