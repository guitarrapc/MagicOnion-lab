using MessagePack;

namespace MagicOnionLab.Shared.Mpos
{
    [MessagePackObject]
    public class PositionRoomJoinRequest
    {
        [Key(0)]
        public string RoomName { get; set; }
        [Key(1)]
        public string UserName { get; set; }
    }
}
