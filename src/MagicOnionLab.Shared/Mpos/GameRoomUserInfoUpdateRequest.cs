using MessagePack;
using UnityEngine;

namespace MagicOnionLab.Shared.Mpos
{
    [MessagePackObject]
    public class GameRoomUserInfoUpdateRequest
    {
        [Key(0)]
        public Vector3 Position { get; set; }
    }
}
