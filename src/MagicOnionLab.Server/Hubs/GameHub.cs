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
    private readonly GameRoomsModel _model;
    private string? _roomName;
    private string? _userName;

    public GameHub(GameRoomsModel model, ILogger<GameHub> logger)
    {
        _logger = logger;
        _model = model;
    }

    public async ValueTask CreateRoomAsync(GameRoomCreateRequest request)
    {
        _logger.LogTrace($"{nameof(CreateRoomAsync)}: {request.RoomName} {request.Capacity}");

        _room = await Group.AddAsync(request.RoomName);
        _roomName = request.RoomName;

        if (_model.TryCreateRoom(request.RoomName, request.Capacity))
        {
            this.Broadcast(_room).OnCreateRoom(request.RoomName);
        }
    }

    public async ValueTask JoinRoomAsync(GameRoomJoinRequest request)
    {
        ArgumentNullException.ThrowIfNull(_room);

        _logger.LogInformation($"{nameof(JoinRoomAsync)}: {request.UserName} {request.RoomName}");

        _userName = request.UserName;
        _model.TryJoinRoom(request.RoomName, request.UserName);
        this.Broadcast(_room).OnJoinRoom(request.UserName);
    }

    public async ValueTask LeaveRoomAsync()
    {
        ArgumentNullException.ThrowIfNull(_room);
        ArgumentNullException.ThrowIfNull(_userName);

        _logger.LogInformation($"{nameof(LeaveRoomAsync)}: {_userName}");

        await _room.RemoveAsync(Context);
        Broadcast(_room).OnLeaveRoom(_userName);
    }

    public async ValueTask ReadyMatchAsync()
    {
        ArgumentNullException.ThrowIfNull(_room);
        ArgumentNullException.ThrowIfNullOrEmpty(_roomName);
        ArgumentNullException.ThrowIfNullOrEmpty(_userName);

        _logger.LogInformation($"{nameof(ReadyMatchAsync)}: {_userName}");

        try
        {
            _model.ReadyMatch(_roomName, _userName, true);
            await _model.WaitMatchingCompletedAsync(_roomName); // wait until all members sent ready for match
            if (_model.TryClearMatchInfo(_roomName))
            {
                this.Broadcast(_room).OnMatchCompleted();
            }
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"Error happen on {_userName} {ex.Message} {ex}");
            throw;
        }
    }

    public ValueTask UpdateUserInfonAsync(GameRoomUserInfoUpdateRequest request)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(_roomName);
        ArgumentNullException.ThrowIfNullOrEmpty(_userName);
        ArgumentNullException.ThrowIfNull(_room);

        _logger.LogTrace($"{nameof(UpdateUserInfonAsync)}: {_userName} => ({request.Position.x},{request.Position.y},{request.Position.z})");

        _model.TryUpdateUserverInfo(_roomName, _userName, request.Position);
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
