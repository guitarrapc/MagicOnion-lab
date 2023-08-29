using MagicOnion;
using MagicOnionLab.Shared.Mpos;
using System.Threading.Tasks;

namespace MagicOnionLab.Shared.Hubs
{
    public interface IGameHub : IStreamingHub<IGameHub, IGameHubReceiver>
    {
        ValueTask CreateRoomAsync(GameRoomCreateRequest request);
        ValueTask JoinRoomAsync(GameRoomJoinRequest request);
        ValueTask ReadyMatchAsync();
        ValueTask LeaveRoomAsync();
        ValueTask UpdateUserInfonAsync(GameRoomUserInfoUpdateRequest request);
    }

    public interface IGameHubReceiver
    {
        void OnCreate(string roomName);
        void OnJoin(string userName);
        void OnLeave(string userName);
        void OnMatch();
        void OnUpdateUserInfo(GameRoomUserInfoUpdateResponse response);
    }
}
