using MagicOnion;
using MessagePack;

namespace MagicOnionLab.Shared.Services
{
    public interface IMathService : IService<IMathService>
    {
        UnaryResult<int> SumAsync(int x, int y);
        UnaryResult<MathServiceMpo> SumMpoAsync(int x, int y);
    }

    [MessagePackObject]
    public class MathServiceMpo
    {
        [Key(0)]
        public int X { get; set; }
        [Key(1)]
        public int Y { get; set; }
        [Key(2)]
        public int Result { get; set; }
    }
}
