using MagicOnionLab.Server.Models;

namespace MagicOnionLab.Server.Infrastructures;

public static class IGameServerBuilderExtensions
{
    public static IGameServerBuilder AddGameServerBuilder(this WebApplicationBuilder builder)
    {
        return new GameServerBuilder(builder.Services, builder.Configuration);
    }

    public static IGameServerBuilder AddGameServer(this IGameServerBuilder builder)
    {
        // Add Models
        builder.Services.AddSingleton<GameRoomsModel>();

        return builder;
    }
}
