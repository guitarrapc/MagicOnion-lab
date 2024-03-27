using Aws.GameLift.Server;
using Aws.GameLift.Server.Model;

namespace GameLiftMagicOnionServer;

public class GameLiftServer : IDisposable
{
    private readonly IConfiguration _configuration;

    public GameLiftServer(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public ServerParameters InitParameters(){
        //WebSocketUrl from RegisterHost call
        var webSocketUrl = _configuration.GetValue<string>("WEBSOCKET_URL", "wss://ap-northeast-1.api.amazongamelift.com");

        //Unique identifier for this process
        var processId = Guid.NewGuid().ToString();

        //Unique identifier for your host that this process belongs to
        var hostId = _configuration.GetValue<string>("HOST_ID", "");

        //Unique identifier for your fleet that this host belongs to
        var fleetId = _configuration.GetValue<string>("FLEET_ID", "");

        //Authentication token for this host process.
        var authToken = _configuration.GetValue<string>("AUTH_TOKEN", "");

        return new ServerParameters(
            webSocketUrl,
            processId,
            hostId,
            fleetId,
            authToken);
    }

    //This is an example of a simple integration with Amazon GameLift server SDK that will make game server processes go active on Amazon GameLift!
    public void Start(int port, ServerParameters serverParameters)
    {
        //Identify port number (hard coded here for simplicity) the game server is listening on for player connections
        var listeningPort = port;

        //InitSDK will establish a local connection with Amazon GameLift's agent to enable further communication.
        var initSDKOutcome = GameLiftServerAPI.InitSDK(serverParameters);
        if (initSDKOutcome.Success)
        {
            ProcessParameters processParameters = new ProcessParameters(
                (GameSession gameSession) =>
                {
                    //When a game session is created, Amazon GameLift sends an activation request to the game server and passes along the game session object containing game properties and other settings.
                    //Here is where a game server should take action based on the game session object.
                    //Once the game server is ready to receive incoming player connections, it should invoke GameLiftServerAPI.ActivateGameSession()
                    Console.WriteLine("ActivateGameSession");
                    GameLiftServerAPI.ActivateGameSession();
                },
                (UpdateGameSession updateGameSession) =>
                {
                    //When a game session is updated (e.g. by FlexMatch backfill), Amazon GameLift sends a request to the game
                    //server containing the updated game session object.  The game server can then examine the provided
                    //matchmakerData and handle new incoming players appropriately.
                    //updateReason is the reason this update is being supplied.
                    Console.WriteLine("updateGameSession");

                },
                () =>
                {
                    //OnProcessTerminate callback. Amazon GameLift will invoke this callback before shutting down an instance hosting this game server.
                    //It gives this game server a chance to save its state, communicate with services, etc., before being shut down.
                    //In this case, we simply tell Amazon GameLift we are indeed going to shutdown.
                    GameLiftServerAPI.ProcessEnding();
                    Console.WriteLine("ProcessEnding");
                },
                () =>
                {
                    //This is the HealthCheck callback.
                    //GameLift will invoke this callback every 60 seconds or so.
                    //Here, a game server might want to check the health of dependencies and such.
                    //Simply return true if healthy, false otherwise.
                    //The game server has 60 seconds to respond with its health status. Amazon GameLift will default to 'false' if the game server doesn't respond in time.
                    //In this case, we're always healthy!
                    Console.WriteLine("healthcheck");
                    return true;
                },
                listeningPort, //This game server tells Amazon GameLift that it will listen on port 7777 for incoming player connections.
                new LogParameters(new List<string>()
                {
                    //Here, the game server tells Amazon GameLift what set of files to upload when the game session ends.
                    //Amazon GameLift will upload everything specified here for the developers to fetch later.
                    "/local/game/logs/myserver.log"
                }));

            //Calling ProcessReady tells Amazon GameLift this game server is ready to receive incoming game sessions!
            var processReadyOutcome = GameLiftServerAPI.ProcessReady(processParameters);
            if (processReadyOutcome.Success)
            {
                Console.WriteLine("ProcessReady success.");
            }
            else
            {
                Console.WriteLine("ProcessReady failure : " + processReadyOutcome.Error.ToString());
            }
        }
        else
        {
            Console.WriteLine("InitSDK failure : " + initSDKOutcome.Error.ToString());
        }
    }

    public void Dispose()
    {
        //Make sure to call GameLiftServerAPI.Destroy() when the application quits. This resets the local connection with Amazon GameLift's agent.
        GameLiftServerAPI.Destroy();
    }
}
