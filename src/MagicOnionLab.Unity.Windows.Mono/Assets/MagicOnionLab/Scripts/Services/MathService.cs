#nullable enable
using MagicOnion.Client;
using MagicOnionLab.Shared.Mpos;
using MagicOnionLab.Shared.Services;
using MagicOnionLab.Unity.Infrastructures;
using MagicOnionLab.Unity.Infrastructures.Defines;
using MagicOnionLab.Unity.Infrastructures.Loggers;
using System.Threading.Tasks;
using UnityEngine;

namespace MagicOnionLab.Unity.Services
{
    public class MathService
    {
        private readonly ILogger _logger;

        public MathService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<int> RequestAsync(int x, int y)
        {
            var channel = await ChannelFactory.GetOrCreateAsync(SystemConstants.ServerUrl);
            var client = MagicOnionClient.Create<IMathService>(channel);

            var sum = await client.SumAsync(x, y);
            _logger.LogInformation($"{nameof(MathService)}.{nameof(client.SumAsync)} '{x} + {y} = {sum}'");

            return sum;
        }

        public async Task<MathResultMpo> RequestMpoAsync(int x, int y)
        {
            var channel = await ChannelFactory.GetOrCreateAsync(SystemConstants.ServerUrl);
            var client = MagicOnionClient.Create<IMathService>(channel);

            var sumMpo = await client.SumMpoAsync(x, y);
            _logger.LogInformation($"{nameof(MathService)}.{nameof(client.SumMpoAsync)} '{x} + {y} = {sumMpo.Result}'");

            return sumMpo;
        }
    }
}
