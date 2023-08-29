using MessagePack;
using UnityEngine;

namespace MagicOnionLab.Shared.Mpos
{
    [MessagePackObject]
    public class GameRoomUserInfoUpdateResponse
    {
        [Key(0)]
        public string UserName { get; set; }
        [Key(1)]
        public Vector3 Position { get; set; }
    }
}
