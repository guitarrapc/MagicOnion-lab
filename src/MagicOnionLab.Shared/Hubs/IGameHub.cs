using MagicOnion;
using MagicOnionLab.Shared.Mpos;
using System.Threading.Tasks;

namespace MagicOnionLab.Shared.Hubs
{
    public interface IGameHub : IStreamingHub<IGameHub, IGameHubReceiver>
    {
        /// <summary>
        /// Create a room
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        ValueTask CreateRoomAsync(GameRoomCreateRequest request);
        /// <summary>
        /// Join user to the Room
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        ValueTask JoinRoomAsync(GameRoomJoinRequest request);
        /// <summary>
        /// Leave a room
        /// </summary>
        /// <returns></returns>
        ValueTask LeaveRoomAsync();
        /// <summary>
        /// Ready matching for the user
        /// </summary>
        /// <returns></returns>
        ValueTask ReadyMatchAsync();
        /// <summary>
        /// Update userinfo for the room
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        ValueTask UpdateUserInfonAsync(GameRoomUserInfoUpdateRequest request);
    }
}
