using MessagePack;

namespace MagicOnionLab.Shared.Mpos
{
    [MessagePackObject]
    public class GameRoomCreateRequest
    {
        [Key(0)]
        public string RoomName { get; set; }
        [Key(1)]
        public int Capacity { get; set; }
    }
}
