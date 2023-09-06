#nullable enable
using MagicOnion.Client;
using MagicOnionLab.Shared.Mpos;
using MagicOnionLab.Shared.Services;
using System.Threading.Tasks;
using UnityEngine;

namespace MagicOnionLab.Unity
{
    public class MathService
    {
        private readonly ILogger _logger;

        public MathService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<MathResultMpo> RequestAsync(int x, int y)
        {
            using var channel = await ChannelFactory.CreateAsync(Constants.ServerUrl);
            var client = MagicOnionClient.Create<IMathService>(channel);

            var sum = await client.SumAsync(x, y);
            _logger.LogInformation($"{nameof(MathService)}.{nameof(client.SumAsync)} '{x} + {y} = {sum}'");

            var sumMpo = await client.SumMpoAsync(x, y);
            _logger.LogInformation($"{nameof(MathService)}.{nameof(client.SumMpoAsync)} '{x} + {y} = {sum}'");

            return sumMpo;
        }
    }
}
