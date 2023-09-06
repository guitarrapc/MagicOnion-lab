#nullable enable
using MagicOnion;
using MagicOnion.Client;
using MagicOnionLab.Shared.Mpos;
using MagicOnionLab.Shared.Services;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace MagicOnionLab.Unity
{
    public class GameComponent : MonoBehaviour
    {
        private UnityEngine.ILogger? _logger = new UnityCustomLogger(true);

        [SerializeField]
        private MathServiceComponentView? _mathServiceComponentView = default;

        private async void Start()
        {
            // MathService
            if (_mathServiceComponentView is not null)
            {
                var mathResult = await MathClientService(_mathServiceComponentView.X, _mathServiceComponentView.Y);
                _mathServiceComponentView.SetResult(mathResult);
            }
            else
            {
                _ = await MathClientService(10, 20);
            }
        }

        public async Task<MathResultMpo> MathClientService(int x, int y)
        {
            using var channel = await TryConnectAsync(Constants.ServerUrl);
            var client = MagicOnionClient.Create<IMathService>(channel);

            var sum = await client.SumAsync(x, y);
            _logger.LogInformation($"{nameof(client.SumAsync)} result: {sum}");

            var sumMpo = await client.SumMpoAsync(x, y);
            _logger.LogInformation($"{nameof(client.SumMpoAsync)} Result: {sumMpo.Result}");

            return sumMpo;
        }

        static async ValueTask<GrpcChannelx> TryConnectAsync(string host, int tryCount = 3, int intervalSec = 3)
        {
            for (var i = 0; i < tryCount; i++)
            {
                try
                {
                    var channel = GrpcChannelx.ForAddress(host);
                    return channel;
                }
                catch (Grpc.Core.RpcException)
                {
                    if (i < tryCount)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(intervalSec));
                        continue;
                    }
                    throw;
                }
            }
            throw new ApplicationException($"Cannot connect to the server. {host}");
        }
    }
}
