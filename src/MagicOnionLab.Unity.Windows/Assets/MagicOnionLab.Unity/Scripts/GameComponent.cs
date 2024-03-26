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
        private GameAdminView? _gameAdminView = default;
        [SerializeField]
        private MathServiceComponentView? _mathServiceComponentView = default;
        [SerializeField]
        private GameHubComponentView? _gameHubComponentView = default;

        private async void Start()
        {
            await HandleGameAdminAsync();
            await HandleMathServiceAsync();
            await HandleGametHubAsync();
        }

        private void OnDestroy()
        {
            if (_cts is not null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }
        }

        private async void OnApplicationQuit()
        {
            _logger.LogInformation("Quitting game.");
            await ChannelFactory.ClearAsync();
        }

        private async Task HandleGameAdminAsync()
        {
            if (_gameAdminView is not null)
            {
                _gameAdminView.RegisterQuitGameClickEvent(() => ApplicationHelper.Quit());
            }
        }

        private async Task HandleMathServiceAsync()
        {
            var mathClient = new MathService(_logger);
            if (_mathServiceComponentView is not null)
            {
                _mathServiceComponentView.Initialize();
                _mathServiceComponentView.RegisterClickEvent(async () =>
                {
                    var mathResult = await mathClient.RequestMpoAsync(_mathServiceComponentView.X, _mathServiceComponentView.Y);
                    _mathServiceComponentView.AppendResult(mathResult);
                    _mathServiceComponentView.ExecutionComplete();
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

        private async Task HandleGametHubAsync()
        {
            if (_gameHubComponentView is not null)
            {
                _gameHubComponentView.Initialize();

                _gameHubComponentView.RegisterClickEvent(async () =>
                {
                    if (!_gameHubComponentView.Executing)
                    {
                        _gameHubComponentView.ExecutionBegin();
                        await ExecuteAsync(_gameHubComponentView.RoomName, _gameHubComponentView.UserCount, _gameHubComponentView.Capacity);
                        _gameHubComponentView.ExecutionComplete();
                        _gameHubComponentView.AppendResult($"Complete.");
                    }
                    else
                    {
                        _gameHubComponentView?.AppendResult($"Already executing, skip request.");
                    }
                });
            }
            else
            {
                await ExecuteAsync("foo", 8, 8);
            }

            async Task ExecuteAsync(string roomName, int userCount, int capacity)
            {
                _gameHubComponentView?.AppendResult($"# Begin with following parameters. roomName {roomName}, userCount {userCount}, capacity {capacity}.");
                var tasks = Enumerable.Range(1, userCount)
                    .Select((x, i) => (userName: $"foo{x}", index: i))
                    .Select(async x =>
                    {
                        await Task.Delay(200 * Random.Range(1, userCount) * x.index);

                        var userName = x.userName;
                        var index = x.index;

                        var channel = await ChannelFactory.GetOrCreateAsync(SystemConstants.ServerUrl);
                        await using var client = new GameHubClient(_logger, text => _gameHubComponentView?.AppendResult(text), userName, index);

                        // connect
                        _gameHubComponentView?.AppendResult($"{userName}@{roomName}: Connecting room.");
                        await client.ConnectAsync(channel, roomName, capacity, destroyCancellationToken);

                        // match
                        _gameHubComponentView?.AppendResult($"{userName}@{roomName}: Matching and send ready.");
                        await client.ReadyAsync();

                        // update
                        _gameHubComponentView?.AppendResult($"{userName}@{roomName}: Updating info.");
                        await client.UpdateUserInfoAsync();

                        // leave
                        _gameHubComponentView?.AppendResult($"{userName}@{roomName}: Leaving room.");
                        await client.LeaveAsync();
                    })
                    .ToArray();
                await Task.WhenAll(tasks);
            }
        }
    }
}
