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

    public interface IGameHubReceiver
    {
        /// <summary>
        /// Notify when Created room
        /// </summary>
        /// <param name="roomName"></param>
        void OnCreateRoom(string roomName);
        /// <summary>
        /// Notify when Joined room
        /// </summary>
        /// <param name="userName"></param>
        void OnJoinRoom(string userName);
        /// <summary>
        /// Notify when leaved room
        /// </summary>
        /// <param name="userName"></param>
        void OnLeaveRoom(string userName);
        /// <summary>
        /// Notify when match complated
        /// </summary>
        void OnMatchCompleted();
        /// <summary>
        /// Notify when updated userinfo
        /// </summary>
        /// <param name="response"></param>
        void OnUpdateUserInfo(GameRoomUserInfoUpdateResponse response);
    }
}
