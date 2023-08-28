using MagicOnion;
using MagicOnionLab.Shared.Mpos;
using System.Threading.Tasks;

namespace MagicOnionLab.Shared.Hubs
{
    public interface IPositionHub : IStreamingHub<IPositionHub, IPositionHubReceiver>
    {
        ValueTask CreateRoomAsync(PositionRoomCreateRequest request);
        ValueTask JoinAsync(PositionRoomJoinRequest request);
        ValueTask ReadyAsync();
        ValueTask LeaveAsync();
        ValueTask UpdatePosition(PositionRoomUpdateRequest request);
    }

    public interface IPositionHubReceiver
    {
        void OnCreate(string roomName);
        void OnJoin(string userName);
        void OnLeave(string userName);
        void OnMatch();
        void OnUpdatePosition(PositionRoomUpdateResponse response);
    }
}
