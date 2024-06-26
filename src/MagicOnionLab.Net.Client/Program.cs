using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnionLab.Shared.Helpers;
using MagicOnionLab.Shared.Hubs;
using MagicOnionLab.Shared.Mpos;
using MagicOnionLab.Shared.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using UnityEngine;
using ValueTaskSupplement;

if (!args.Any())
{
    //args = new[] { "math", "--host", "http://localhost:5288", "--x", "123", "--y", "578" };
    //args = new[] { "game", "--host", "http://localhost:5288", "--room-name", "room1", "--user-count", "1", "--capacity", "1" };
    //args = new[] { "game", "--host", "http://localhost:5288", "--room-name", "room1", "--user-count", "2", "--capacity", "2" };
    //args = new[] { "game", "--host", "http://localhost:5288", "--room-name", "room1", "--user-count", "4", "--capacity", "4" };
    //args = new[] { "game", "--host", "http://localhost:5288", "--room-name", "room1", "--user-count", "8", "--capacity", "8" };
    args = new[] { "all", "--host", "http://localhost:5288" };
}

var app = ConsoleApp.Create(args);
app.AddCommands<MagicOnionClientApp>();
app.Run();

public class MagicOnionClientApp : ConsoleAppBase
{
    [Command("all")]
    public async ValueTask All(string host)
    {
        await MathClientService(host, Random.Shared.Next(), Random.Shared.Next());
        var room1 = GameClientHub(host, "room1", 1, 1);
        var room2 = GameClientHub(host, "room2", 2, 2);
        var room3 = GameClientHub(host, "room3", 4, 4);
        var room4 = GameClientHub(host, "room4", 8, 8);
        await ValueTaskEx.WhenAll(room1, room2, room3, room4);
    }

    [Command("math")]
    public async ValueTask MathClientService(string host, int x, int y)
    {
        var channel = await ChannelFactory.GetOrCreateAsync(host);
        var client = MagicOnionClient.Create<IMathService>(channel);

        var sum = await client.SumAsync(x, y);
        Context.Logger.LogInformation($"{nameof(client.SumAsync)} result: {sum}");

        var sumMpo = await client.SumMpoAsync(x, y);
        Context.Logger.LogInformation($"{nameof(client.SumMpoAsync)} Result: {sumMpo.Result}");
    }

    [Command(commandName: "game")]
    public async ValueTask GameClientHub(string host, string roomName, int userCount = 1, int capacity = 1)
    {
        var tasks = Enumerable.Range(1, userCount)
            .Select((x, i) => (userName: UserNameGenerator.GetRandomtName(), index: i))
            .Select(async x =>
            {
                await Task.Delay(200 * Random.Shared.Next(1, userCount) * x.index);

                var userName = x.userName;
                var index = x.index;

                var channel = await ChannelFactory.GetOrCreateAsync(host);
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
    private string? _roomName;
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
        _roomName = roomName;
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
            _logger.LogInformation($"{_userName}@{_roomName}: Disconnected from the server.");

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
        ArgumentNullException.ThrowIfNull(_channel);

        _logger.LogInformation($"{_userName}@{_roomName}: Reconnecting to the server.");
        _client = await StreamingHubClient.ConnectAsync<IGameHub, IGameHubReceiver>(_channel, this);
        RegisterDisconnect(_client).FireAndForget(_logger);
        _logger.LogInformation($"{_userName}@{_roomName}: Reconnected.");

        _isSelfDisConnected = false;
    }


    public async ValueTask ReadyAsync()
    {
        ArgumentNullException.ThrowIfNull(_client);

        // ready
        _logger.LogInformation($"{_userName}@{_roomName}: Matching and send ready.");
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
        _logger.LogInformation($"{_userName}@{_roomName}: Room created. (Event)");
    }

    public void OnJoinRoom(string userName)
    {
        if (_userName.Equals(userName, StringComparison.Ordinal))
        {
            _logger.LogInformation($"{_userName}@{_roomName}: User Join room. (Event)");
        }
    }

    public void OnLeaveRoom(string userName)
    {
        if (_userName.Equals(userName, StringComparison.Ordinal))
        {
            _logger.LogInformation($"{_userName}@{_roomName}: User leave. (Event)");
        }
    }

    public void OnMatchCompleted()
    {
        _logger.LogInformation($"{_userName}@{_roomName}: Matching complete. (Event)");
    }

    public void OnUpdateUserInfo(GameRoomUserInfoUpdateResponse response)
    {
        if (_userName.Equals(response.UserName, StringComparison.Ordinal))
        {
            _logger.LogInformation($"{_userName}@{_roomName}: Update UserInfo. position {{{response.Position.x},{response.Position.y},{response.Position.z}}} (Event)");
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

public static class ChannelFactory
{
    private static ConcurrentDictionary<string, GrpcChannel> _channelHolder = new ConcurrentDictionary<string, GrpcChannel>();

    /// <summary>
    /// Create GrpcChannelx or Get from cache. Please do not Dispose GrpcChannelx obtained from this factory.
    /// </summary>
    /// <param name="host"></param>
    /// <param name="tryCount"></param>
    /// <param name="intervalSec"></param>
    /// <returns></returns>
    /// <exception cref="ApplicationException"></exception>
    public static async ValueTask<GrpcChannel> GetOrCreateAsync(string host, int tryCount = 3, int intervalSec = 3)
    {
        if (_channelHolder.TryGetValue(host, out var cached))
        {
            return cached;
        }

        for (var i = 0; i < tryCount; i++)
        {
            try
            {
                var channel = GrpcChannel.ForAddress(host);
                if (_channelHolder.TryAdd(host, channel))
                {
                    return channel;
                }
                else
                {
                    // duplicated, dispose it.
                    channel.Dispose();

                    // re-obtain already in-use channel
                    if (_channelHolder.TryGetValue(host, out var cached2))
                    {
                        return cached2;
                    }
                }
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

    public static void Clear()
    {
        foreach (var channel in _channelHolder)
        {
            channel.Value.Dispose();
        }

        _channelHolder.Clear();
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
