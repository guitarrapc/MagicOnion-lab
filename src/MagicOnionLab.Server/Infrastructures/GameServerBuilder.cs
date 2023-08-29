namespace MagicOnionLab.Server.Infrastructures;

public interface IGameServerBuilder
{
    IServiceCollection Services { get; }
    IConfigurationBuilder Configuration { get; }
}

public class GameServerBuilder : IGameServerBuilder
{
    public IServiceCollection Services { get; }
    public IConfigurationBuilder Configuration { get; }

    public GameServerBuilder(IServiceCollection services, IConfigurationBuilder configuration)
    {
        Services = services;
        Configuration = configuration;
    }
}
