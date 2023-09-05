using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnionLab.Shared.Hubs;
using MagicOnionLab.Shared.Mpos;
using MagicOnionLab.Shared.Services;
using Microsoft.Extensions.Logging;
using UnityEngine;

//args = new[] { "math", "--host", "http://localhost:5288", "--x", "123", "--y", "578" };
//args = new[] { "position", "--host", "http://localhost:5288", "--room-name", "room1", "--user-count", "1", "--capacity", "1" };
//args = new[] { "position", "--host", "http://localhost:5288", "--room-name", "room1", "--user-count", "2", "--capacity", "2" };
//args = new[] { "position", "--host", "http://localhost:5288", "--room-name", "room1", "--user-count", "4", "--capacity", "4" };
args = new[] { "position", "--host", "http://localhost:5288", "--room-name", "room1", "--user-count", "8", "--capacity", "8" };

var app = ConsoleApp.Create(args);
app.AddCommands<MagicOnionClientApp>();
app.Run();

public class MagicOnionClientApp : ConsoleAppBase
{
    [Command("math")]
    public async ValueTask MathClientService(string host, int x, int y)
    {
        using var channel = await TryConnectAsync(host);
        var client = MagicOnionClient.Create<IMathService>(channel);

        var sum = await client.SumAsync(x, y);
        Console.WriteLine($"{nameof(client.SumAsync)} result: {sum}");

        var sumMpo = await client.SumMpoAsync(x, y);
        Console.WriteLine($"{nameof(client.SumMpoAsync)} Result: {sumMpo.Result}");
    }

    [Command(commandName: "position")]
    public async ValueTask PositionClientHub(string host, string roomName, int userCount = 1, int capacity = 1)
    {
        if (userCount == 1)
        {
            var index = 0;
            var userName = "foo";
            using var channel = await TryConnectAsync(host);
            await using var client = new GameHubClient(Context.Logger, userName, index);

            // connect
            await client.ConnectAsync(channel, roomName, capacity, Context.CancellationToken);

            // match
            await client.ReadyAsync();

            // update
            await client.UpdateUserInfoAsync();

            // leave
            await client.LeaveAsync();
        }
        else
        {
            var tasks = Enumerable.Range(1, userCount)
                .Select((x, i) => (userName: $"foo{x}", index: i))
                .Select(async x =>
                {
                    await Task.Delay(200 * Random.Shared.Next(1, userCount) * x.index);

                    var userName = x.userName;
                    var index = x.index;

                    using var channel = await TryConnectAsync(host);
                    await using var client = new GameHubClient(Context.Logger, userName, index);

                    // connect
                    await client.ConnectAsync(channel, roomName, capacity, Context.CancellationToken);

                    // match
                    await client.ReadyAsync();

                    // update
                    await client.UpdateUserInfoAsync();

                    // leave
                    await client.LeaveAsync();
                })
                .ToArray();
            await Task.WhenAll(tasks);
        }

        Context.Logger.LogInformation("complete.");
    }

    static async ValueTask<GrpcChannel> TryConnectAsync(string host, int tryCount = 3, int intervalSec = 3)
    {
        for (var i = 0; i < tryCount; i++)
        {
            try
            {
                var channel = GrpcChannel.ForAddress(host);
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

public class GameHubClient : IGameHubReceiver, IAsyncDisposable
{
    private GrpcChannel? _channel;
    private IGameHub? _client;
    private readonly ILogger _logger;
    private readonly string _userName;
    private int _index;
    private bool _isSelfDisConnected;

    public GameHubClient(ILogger logger, string userName, int index)
    {
        _logger = logger;
        _userName = userName;
        _index = index;
    }

    public async ValueTask ConnectAsync(GrpcChannel channel, string roomName, int capacity, CancellationToken ct)
    {
        _channel = channel;
        _client = await StreamingHubClient.ConnectAsync<IGameHub, IGameHubReceiver>(channel, this, cancellationToken: ct);
        RegisterDisconnect(_client).FireAndForget(_logger);

        // create
        await _client.CreateRoomAsync(new GameRoomCreateRequest
        {
            RoomName = roomName,
            Capacity = capacity,
        });

        // join
        await _client.JoinRoomAsync(new GameRoomJoinRequest
        {
            RoomName = roomName,
            UserName = _userName,
        });
    }

    public async Task RegisterDisconnect(IGameHub client)
    {
        try
        {
            // you can wait disconnected event
            await client.WaitForDisconnect();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
        finally
        {
            // try-to-reconnect? logging event? close? etc...
            _logger.LogInformation($"Disconnected from the server.");

            if (_isSelfDisConnected)
            {
                // there is no particular meaning
                await Task.Delay(2000);

                // reconnect
                await ReconnectServerAsync();
            }
        }
    }

    private async Task ReconnectServerAsync()
    {
        _logger.LogInformation($"Reconnecting to the server...");
        _client = await StreamingHubClient.ConnectAsync<IGameHub, IGameHubReceiver>(_channel, this);
        RegisterDisconnect(_client).FireAndForget(_logger);
        _logger.LogInformation("Reconnected.");

        _isSelfDisConnected = false;
    }


    public async ValueTask ReadyAsync()
    {
        ArgumentNullException.ThrowIfNull(_client);

        // ready
        _logger.LogInformation($"Ready matching: {_userName}");
        await _client.ReadyMatchAsync();
    }

    public async ValueTask UpdateUserInfoAsync()
    {
        ArgumentNullException.ThrowIfNull(_client);

        // update
        for (var i = 0; i < 10; i++)
        {
            var position = i * (_index + 1);
            await _client.UpdateUserInfonAsync(new GameRoomUserInfoUpdateRequest
            {
                Position = new Vector3(position, position, position),
            });
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }

    public async ValueTask LeaveAsync()
    {
        ArgumentNullException.ThrowIfNull(_client);
        await _client.LeaveRoomAsync();
        _isSelfDisConnected = true;
    }

    public void OnCreateRoom(string roomName)
    {
        _logger.LogInformation($"Create room: {roomName}");
    }

    public void OnJoinRoom(string userName)
    {
        if (_userName.Equals(userName, StringComparison.Ordinal))
        {
            _logger.LogInformation($"Join user: {userName}");
        }
    }

    public void OnLeaveRoom(string userName)
    {
        if (_userName.Equals(userName, StringComparison.Ordinal))
        {
            _logger.LogInformation($"Leave user: {userName}");
        }
    }

    public void OnMatchCompleted()
    {
        _logger.LogInformation($"Matching complete.");
    }

    public void OnUpdateUserInfo(GameRoomUserInfoUpdateResponse response)
    {
        if (_userName.Equals(response.UserName, StringComparison.Ordinal))
        {
            _logger.LogInformation($"Update UserInfo: userName {response.UserName}, position: ({response.Position.x},{response.Position.y},{response.Position.z})");
        }
    }

    public Task DisposeAsync()
    {
        if (_client is not null)
        {
            return _client.DisposeAsync();
        }
        return Task.CompletedTask;
    }

    // You can watch connection state, use this for retry etc.
    public Task WaitForDisconnect()
    {
        if (_client is not null)
        {
            return _client.WaitForDisconnect();
        }
        return Task.CompletedTask;
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (_client is not null )
        {
            await _client.DisposeAsync();
        }
    }
}

public static class TaskExtensions
{
    public static void FireAndForget(this Task task, ILogger logger)
    {
        task.ContinueWith(x =>
        {
            logger.LogError(x.Exception, "TaskUnhandled");
        }, TaskContinuationOptions.OnlyOnFaulted);
    }
}
