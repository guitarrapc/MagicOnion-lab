#nullable enable
using MagicOnionLab.Unity.Hubs;
using MagicOnionLab.Unity.Infrastructures;
using MagicOnionLab.Unity.Infrastructures.Defines;
using MagicOnionLab.Unity.Infrastructures.Loggers;
using MagicOnionLab.Unity.Services;
using MagicOnionLab.Unity.Views;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MagicOnionLab.Unity
{
    public class GameComponent : MonoBehaviour
    {
        private UnityEngine.ILogger _logger = new UnityCustomLogger(true);
        private CancellationTokenSource _cts = new CancellationTokenSource(System.TimeSpan.FromMinutes(10));

        [SerializeField]
        private MathServiceComponentView? _mathServiceComponentView = default;

        private async void Start()
        {
            await MathServiceAsync();
            await GameCLientHubAsync(SystemConstants.ServerUrl, "foo", 8, 8);
        }

        private void OnDestroy()
        {
            if (_cts is not null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }
        }

        private async Task MathServiceAsync()
        {
            var mathClient = new MathService(_logger);
            if (_mathServiceComponentView is not null)
            {
                _mathServiceComponentView.RegisterClickEvent(async () =>
                {
                    var mathResult = await mathClient.RequestMpoAsync(_mathServiceComponentView.X, _mathServiceComponentView.Y);
                    _mathServiceComponentView.SetResult(mathResult);
                });
            }
            else
            {
                for (var i = 0; i < 5; i++)
                {
                    var x = Random.Range(10, 9999);
                    var y = Random.Range(10, 9999);
                    _ = await mathClient.RequestMpoAsync(x, y);
                }
            }
        }

        private async Task GameCLientHubAsync(string host, string roomName, int userCount = 1, int capacity = 1)
        {
            var tasks = Enumerable.Range(1, userCount)
                .Select((x, i) => (userName: $"foo{x}", index: i))
                .Select(async x =>
                {
                    await Task.Delay(200 * Random.Range(1, userCount) * x.index);

                    var userName = x.userName;
                    var index = x.index;

                    var channel = await ChannelFactory.GetOrCreateAsync(host);
                    await using var client = new GameHubClient(_logger, userName, index);

                    // connect
                    await client.ConnectAsync(channel, roomName, capacity, destroyCancellationToken);

                    // match
                    await client.ReadyAsync();

                    // update
                    await client.UpdateUserInfoAsync();

                    // leave
                    await client.LeaveAsync();
                })
                .ToArray();
            await Task.WhenAll(tasks);

            _logger.LogInformation("complete.");
        }
    }
}
