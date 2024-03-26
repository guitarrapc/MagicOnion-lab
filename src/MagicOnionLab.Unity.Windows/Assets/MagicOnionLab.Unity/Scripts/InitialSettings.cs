using System.IO;
using MagicOnion.Client;
using Grpc.Net.Client;
using MagicOnion.Unity;
using MessagePack;
using MessagePack.Resolvers;
using UnityEngine;

namespace MagicOnionLab.Unity
{
    [MagicOnionClientGeneration(typeof(MagicOnionLab.Shared.Services.IMathService), typeof(MagicOnionLab.Shared.Hubs.IGameHub))]
    internal partial class MagicOnionClientInitializer
    {
    }

    class InitialSettings
    {
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void RegisterResolvers()
        {
            StaticCompositeResolver.Instance.Register(
                // Add: Use MessagePack formatter resolver generated by the source generator.
                MagicOnionClientInitializer.Resolver,
                MessagePack.Resolvers.GeneratedResolver.Instance,
                BuiltinResolver.Instance,
                PrimitiveObjectResolver.Instance,
                MessagePack.Unity.UnityResolver.Instance
            );

            MessagePackSerializer.DefaultOptions = MessagePackSerializer.DefaultOptions
                .WithResolver(StaticCompositeResolver.Instance);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void OnRuntimeInitialize()
        {
            // Use Grpc.Net.Client instead of C-core gRPC library.
            GrpcChannelProviderHost.Initialize(
                new GrpcNetClientGrpcChannelProvider(() => new GrpcChannelOptions  ()
                {
                    HttpHandler = new Cysharp.Net.Http.YetAnotherHttpHandler()
                    {
                        Http2Only = true,
                    }
                }));
        }
    }
}
