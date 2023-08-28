using MessagePack;

namespace MagicOnionLab.Shared.Mpos
{
    [MessagePackObject]
    public class MathResultMpo
    {
        [Key(0)]
        public int X { get; set; }
        [Key(1)]
        public int Y { get; set; }
        [Key(2)]
        public int Result { get; set; }
    }
}
