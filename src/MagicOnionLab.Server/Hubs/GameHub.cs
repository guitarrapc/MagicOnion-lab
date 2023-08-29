#pragma warning disable CS8644 // Type does not implement interface member. Nullability of reference types in interface implemented by the base type doesn't match.
using MagicOnion.Server.Hubs;
using MagicOnionLab.Server.Models;
using MagicOnionLab.Shared.Hubs;
using MagicOnionLab.Shared.Mpos;

namespace MagicOnionLab.Server.Hubs;

public class GameHub : StreamingHubBase<IGameHub, IGameHubReceiver>, IGameHub
{
    private readonly ILogger<GameHub> _logger;
    private IGroup? _room;
    private readonly GameRoomsModel _rooms;
    private string? _roomName;
    private string? _userName;

    public GameHub(GameRoomsModel rooms, ILogger<GameHub> logger)
    {
        _logger = logger;
        _rooms = rooms;
    }

    public async ValueTask CreateRoomAsync(GameRoomCreateRequest request)
    {
        _room = await Group.AddAsync(request.RoomName);
        _roomName = request.RoomName;

        if (_rooms.TryCreate(request.RoomName, request.Capacity))
        {
            this.Broadcast(_room).OnCreate(request.RoomName);
        }
    }

    public async ValueTask JoinRoomAsync(GameRoomJoinRequest request)
    {
        ArgumentNullException.ThrowIfNull(_room);

        _userName = request.UserName;
        _rooms.TryJoinRoom(request.RoomName, request.UserName);
        this.Broadcast(_room).OnJoin(request.UserName);
    }

    public async ValueTask LeaveRoomAsync()
    {
        ArgumentNullException.ThrowIfNull(_room);
        await _room.RemoveAsync(Context);
        Broadcast(_room).OnLeave(_userName);
    }

    public async ValueTask ReadyMatchAsync()
    {
        ArgumentNullException.ThrowIfNull(_room);
        ArgumentNullException.ThrowIfNullOrEmpty(_roomName);
        ArgumentNullException.ThrowIfNullOrEmpty(_userName);
        _rooms.TryReadyMatch(_roomName, _userName);
        if (_rooms.IsMatchComplete(_roomName))
        {
            _rooms.ClearMatchInfo(_roomName);
            this.Broadcast(_room).OnMatch();
        }
    }

    public ValueTask UpdateUserInfonAsync(GameRoomUserInfoUpdateRequest request)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(_roomName);
        ArgumentNullException.ThrowIfNullOrEmpty(_userName);
        ArgumentNullException.ThrowIfNull(_room);

        _logger.LogInformation($"{nameof(UpdateUserInfonAsync)}: {_userName} => ({request.Position.x},{request.Position.y},{request.Position.z})");
        _rooms.TryUpdateUserverInfo(_roomName, _userName, request.Position);
        Broadcast(_room).OnUpdateUserInfo(new GameRoomUserInfoUpdateResponse
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
