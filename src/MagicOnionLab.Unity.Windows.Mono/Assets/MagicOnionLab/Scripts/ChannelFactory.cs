#nullable enable
using MagicOnion;
using System;
using System.Threading.Tasks;

namespace MagicOnionLab.Unity
{
    public static class ChannelFactory
    {
        public static async ValueTask<GrpcChannelx> CreateAsync(string host, int tryCount = 3, int intervalSec = 3)
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
