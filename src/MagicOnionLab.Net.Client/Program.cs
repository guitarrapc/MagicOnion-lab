using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnionLab.Shared.Hubs;
using MagicOnionLab.Shared.Mpos;
using MagicOnionLab.Shared.Services;
using Microsoft.Extensions.Logging;
using UnityEngine;

//args = new[] { "math", "--host", "http://localhost:5288", "--x", "123", "--y", "578" };
//args = new[] { "position", "--host", "http://localhost:5288", "--room-name", "room1", "--user-count", "1" };
//args = new[] { "position", "--host", "http://localhost:5288", "--room-name", "room1", "--user-count", "2" };
args = new[] { "position", "--host", "http://localhost:5288", "--room-name", "room1", "--user-count", "4" };

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
    public async ValueTask PositionClientHub(string host, string roomName, int userCount = 1)
    {
        if (userCount == 1)
        {
            var index = 0;
            var userName = "foo";
            using var channel = await TryConnectAsync(host);
            var client = new PositionHubClient(Context.Logger, userName, index);

            // connect
            await client.ConnectAsync(channel, roomName, userCount, Context.CancellationToken);

            // match
            await client.ReadyAsync();

            // update
            await client.UpdatePositionAsync();

            // leave
            await client.LeaveAsync();
        }
        else
        {
            var tasks = Enumerable.Range(1, userCount)
                .Select((x, i) => (userName: $"foo{x}", index: i))
                .Select(async x =>
                {
                    await Task.Delay(1000 * x.index);

                    var userName = x.userName;
                    var index = x.index;

                    using var channel = await TryConnectAsync(host);
                    var client = new PositionHubClient(Context.Logger, userName, index);

                    // connect
                    await client.ConnectAsync(channel, roomName, userCount, Context.CancellationToken);

                    // match
                    await client.ReadyAsync();

                    // update
                    await client.UpdatePositionAsync();

                    // leave
                    await client.LeaveAsync();
                })
                .ToArray();
            await Task.WhenAll(tasks);
        }
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

public class PositionHubClient : IPositionHubReceiver
{
    public Task<bool> OnMatchComplete => matchTcs.Task;
    private TaskCompletionSource<bool> matchTcs = new TaskCompletionSource<bool>();

    private IPositionHub? _client;
    private readonly ILogger _logger;
    private readonly string _userName;
    private int _index;

    public PositionHubClient(ILogger logger, string userName, int index)
    {
        _logger = logger;
        _userName = userName;
        _index = index;
    }

    public async ValueTask ConnectAsync(GrpcChannel channel, string roomName, int capacity, CancellationToken ct)
    {
        _client = await StreamingHubClient.ConnectAsync<IPositionHub, IPositionHubReceiver>(channel, this, cancellationToken: ct);

        // create
        await _client.CreateRoomAsync(new PositionRoomCreateRequest
        {
            RoomName = roomName,
            Capacity = capacity,
        });

        // join
        await _client.JoinAsync(new PositionRoomJoinRequest
        {
            RoomName = roomName,
            UserName = _userName,
        });
    }

    public async ValueTask ReadyAsync()
    {
        ArgumentNullException.ThrowIfNull(_client);

        // ready
        await _client.ReadyAsync();
        await OnMatchComplete; // wait all member ready
    }

    public async ValueTask UpdatePositionAsync()
    {
        ArgumentNullException.ThrowIfNull(_client);

        // update
        for (var i = 0; i < 10; i++)
        {
            await _client.UpdatePosition(new PositionRoomUpdateRequest
            {
                Position = new Vector3(i * (_index + 1), i * (_index + 1), i * (_index + 1)),
            });
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }

    public async ValueTask LeaveAsync()
    {
        ArgumentNullException.ThrowIfNull(_client);
        await _client.LeaveAsync();
    }

    public void OnCreate(string roomName)
    {
        _logger.LogInformation($"Create room: {roomName}");
    }

    public void OnJoin(string userName)
    {
        if (_userName.Equals(userName, StringComparison.Ordinal))
        {
            _logger.LogInformation($"Join user: {userName}");
        }
    }

    public void OnLeave(string userName)
    {
        if (_userName.Equals(userName, StringComparison.Ordinal))
        {
            _logger.LogInformation($"Leave user: {userName}");
        }
    }

    public void OnMatch()
    {
        _logger.LogInformation($"Matching complete.");
        matchTcs.TrySetResult(true);
    }

    public void OnUpdatePosition(PositionRoomUpdateResponse response)
    {
        if (_userName.Equals(response.UserName, StringComparison.Ordinal))
        {
            _logger.LogInformation($"Update Position: userName {response.UserName}, position: ({response.Position.x},{response.Position.y},{response.Position.z})");
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
}
