using Grpc.Net.Client;
using MagicOnion.Client;
using MagicOnionLab.Shared.Services;

args = new[] { "math", "--host", "http://localhost:5288", "--x", "123", "--y", "578"};

var app = ConsoleApp.Create(args);
app.AddCommand("math", async (string host, int x, int y) => await MathClientService(host, x, y));
app.Run();

static async ValueTask MathClientService(string host, int x, int y)
{
    using var channel = await TryConnectAsync(host);
    var mathClient = MagicOnionClient.Create<IMathService>(channel);

    var sum = await mathClient.SumAsync(x, y);
    Console.WriteLine($"{nameof(mathClient.SumAsync)} result: {sum}");

    var sumMpo = await mathClient.SumMpoAsync(x, y);
    Console.WriteLine($"{nameof(mathClient.SumMpoAsync)} Result: {sumMpo.Result}");
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
