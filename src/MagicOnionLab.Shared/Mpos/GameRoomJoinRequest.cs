using MessagePack;

namespace MagicOnionLab.Shared.Mpos
{
    [MessagePackObject]
    public class GameRoomJoinRequest
    {
        [Key(0)]
        public string RoomName { get; set; }
        [Key(1)]
        public string UserName { get; set; }
    }
}
