using MessagePack;

namespace MagicOnionLab.Shared.Mpos
{
    [MessagePackObject]
    public class PositionRoomCreateRequest
    {
        [Key(0)]
        public string RoomName { get; set; }
        [Key(1)]
        public int Capacity { get; set; }
    }
}
