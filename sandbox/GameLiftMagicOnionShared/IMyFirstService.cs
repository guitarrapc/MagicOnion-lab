using MagicOnion;

namespace GameLiftMagicOnionShared
{
  public interface IMyFirstService : IService<IMyFirstService>
  {
      UnaryResult<int> SumAsync(int x, int y);
  }
}
