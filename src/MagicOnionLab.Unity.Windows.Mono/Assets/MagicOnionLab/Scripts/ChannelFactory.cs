#nullable enable
using MagicOnion;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MagicOnionLab.Unity
{
    public static class ChannelFactory
    {
        private static ConcurrentDictionary<string, GrpcChannelx> _channelHolder = new ConcurrentDictionary<string, GrpcChannelx>();

        /// <summary>
        /// Create GrpcChannelx or Get from cache. Please do not Dispose GrpcChannelx obtained from this factory.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="tryCount"></param>
        /// <param name="intervalSec"></param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        public static async ValueTask<GrpcChannelx> GetOrCreateAsync(string host, int tryCount = 3, int intervalSec = 3)
        {
            if (_channelHolder.TryGetValue(host, out var cached))
            {
                return cached;
            }

            for (var i = 0; i < tryCount; i++)
            {
                try
                {
                    var channel = GrpcChannelx.ForAddress(host);
                    if (_channelHolder.TryAdd(host, channel))
                    {
                        return channel;
                    }
                    else
                    {
                        // duplicated, dispose it.
                        await channel.DisposeAsync();

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

        public static async Task ClearAsync()
        {
            foreach (var channel in _channelHolder)
            {
                await channel.Value.DisposeAsync();
            }

            _channelHolder.Clear();
        }
    }
}
