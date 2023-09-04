using MagicOnionLab.Shared.Mpos;

namespace MagicOnionLab.Shared.Hubs
{
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
