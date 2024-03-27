using Grpc.Net.Client;
using MagicOnion.Client;
using GameLiftMagicOnionShared;

var channel = GrpcChannel.ForAddress("http://localhost:5039");
var client = MagicOnionClient.Create<IMyFirstService>(channel);

var result = await client.SumAsync(123, 456);
Console.WriteLine($"Result: {result}");
