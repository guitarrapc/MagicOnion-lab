using MagicOnion;
using MagicOnionLab.Shared.Mpos;

namespace MagicOnionLab.Shared.Services
{
    public interface IMathService : IService<IMathService>
    {
        UnaryResult<int> SumAsync(int x, int y);
        UnaryResult<MathResultMpo> SumMpoAsync(int x, int y);
    }
}
