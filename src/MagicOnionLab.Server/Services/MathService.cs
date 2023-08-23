#pragma warning disable CS8644 // Type does not implement interface member. Nullability of reference types in interface implemented by the base type doesn't match.
using MagicOnion;
using MagicOnion.Server;
using MagicOnionLab.Shared.Services;

namespace MagicOnionLab.Server.Services;

public class MathService : ServiceBase<IMathService>, IMathService
{
    private readonly ILogger<MathService> _logger;

    public MathService(ILogger<MathService> logger)
    {
        _logger = logger;
    }

    public async UnaryResult<int> SumAsync(int x, int y)
    {
        _logger.LogDebug("{nameof} called. x: {x}, y: {y}.", nameof(SumAsync), x, y);
        return x + y;
    }

    public async UnaryResult<MathServiceMpo> SumMpoAsync(int x, int y)
    {
        _logger.LogDebug("{nameof} called. x: {x}, y: {y}.", nameof(SumMpoAsync), x, y);
        return new MathServiceMpo
        {
            X = x,
            Y = y,
            Result = x + y,
        };
    }
}
