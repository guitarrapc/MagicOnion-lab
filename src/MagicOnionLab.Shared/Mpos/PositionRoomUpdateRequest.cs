using MessagePack;
using UnityEngine;

namespace MagicOnionLab.Shared.Mpos
{
    [MessagePackObject]
    public class PositionRoomUpdateRequest
    {
        [Key(0)]
        public Vector3 Position { get; set; }
    }
}
