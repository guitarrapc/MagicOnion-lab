# GameLiftServer

1. Create new ClassLibrary Shared Project

  ```shell
  dotnet new classlib -o sandbox/GameLiftMagicOnionShared -f netstandard2.1
  cd sandbox/GameLiftMagicOnionShared
    dotnet add package MagicOnion.Abstractions
    dotnet add package MessagePack
  cd ../..
  rm sandbox/GameLiftMagicOnionShared/Class1.cs
  cat <<EOF > sandbox/GameLiftMagicOnionShared/IMyFirstService.cs
  using MagicOnion;

  namespace GameLiftMagicOnionShared
  {
    public interface IMyFirstService : IService<IMyFirstService>
    {
        UnaryResult<int> SumAsync(int x, int y);
    }
  }
  EOF
  ```

2. Create new MagicOnion Server project.

  ```shell
  dotnet new grpc -o sandbox/GameLiftMagicOnionServer
  cd sandbox/GameLiftMagicOnionServer
    dotnet add package MagicOnion.Server
    dotnet add reference ../GameLiftMagicOnionShared
  cd ../..
  cat <<EOF > sandbox/GameLiftMagicOnionServer/MyFirstService.cs
  using MagicOnion;
  using MagicOnion.Server;
  using GameLiftMagicOnionShared;

  namespace GameLiftMagicOnionServer;

  public class MyFirstService : ServiceBase<IMyFirstService>, IMyFirstService
  {
      public async UnaryResult<int> SumAsync(int x, int y)
      {
          Console.WriteLine($"Received:{x}, {y}");
          return x + y;
      }
  }
  EOF

  cat <<EOF > sandbox/GameLiftMagicOnionServer/Program.cs
  using GameLiftMagicOnionServer.Services;

  var builder = WebApplication.CreateBuilder(args);

  // Add services to the container.
  builder.Services.AddGrpc();
  builder.Services.AddMagicOnion();

  var app = builder.Build();

  // Configure the HTTP request pipeline.
  app.MapGrpcService<GreeterService>();
  app.MapMagicOnionService();
  app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

  app.Run();
  EOF
  ```

3. Create new MagicOnion Client project.

  ```shell
  dotnet new console -o sandbox/GameLiftMagicOnionClient
  cd sandbox/GameLiftMagicOnionClient
    dotnet add package MagicOnion.Client
    dotnet add reference ../GameLiftMagicOnionShared
  cd ../..
  cat <<EOF > sandbox/GameLiftMagicOnionClient/Program.cs
  using Grpc.Net.Client;
  using MagicOnion.Client;
  using GameLiftMagicOnionShared;

  var channel = GrpcChannel.ForAddress("http://localhost:5039");
  var client = MagicOnionClient.Create<IMyFirstService>(channel);

  var result = await client.SumAsync(123, 456);
  Console.WriteLine($"Result: {result}");
  EOF
  ```

4. Run the server and client.

  ```shell
  dotnet run --project sandbox/GameLiftMagicOnionServer/GameLiftMagicOnionServer.csproj
  dotnet run --project sandbox/GameLiftMagicOnionClient/GameLiftMagicOnionClient.csproj
  ```

  Press `Ctrl+C` to stop Server when success.


5. Download latest GameLiftServer SDK from [here](https://aws.amazon.com/gamelift/getting-started/).

  ```shell
  curl -Lo sandbox/GameLift-CSharp-ServerSDK-5.1.2.zip https://gamelift-server-sdk-release.s3.us-west-2.amazonaws.com/csharp/GameLift-CSharp-ServerSDK-5.1.2.zip
  unzip -o sandbox/GameLift-CSharp-ServerSDK-5.1.2.zip -d sandbox/GameLift-CSharp-ServerSDK
  dotnet build -f net6.0 -c Debug sandbox/GameLift-CSharp-ServerSDK/src/GameLiftServerSDK.sln -p ManagePackageVersionsCentrally=false
  mkdir -p sandbox/GameLiftMagicOnionServer/lib/
  cp sandbox/GameLift-CSharp-ServerSDK/src/src/GameLiftServerSDK/bin/x64/Debug/net6.0/GameLiftServerSDK.dll sandbox/GameLiftMagicOnionServer/lib/.
  ```

6. Add GameLiftSDK to Server project. Mofify `sandbox/GameLiftMagicOnionServer/GameLiftMagicOnionServer.csproj` to use GameLiftServerSDK, add GameLiftSDK using nuget packages.

  ```shell
  sed -i '/<\/PropertyGroup>/i \
    <PlatformTarget>x64</PlatformTarget>' sandbox/GameLiftMagicOnionServer/GameLiftMagicOnionServer.csproj
  sed -i 's/<\/Project>/  <ItemGroup>\
      <Reference Include="GameLiftServerSDK">\
        <HintPath>lib\/GameLiftServerSDK.dll<\/HintPath>\
      <\/Reference>\
    <\/ItemGroup>\
  <\/Project>/g' sandbox/GameLiftMagicOnionServer/GameLiftMagicOnionServer.csproj

  cd sandbox/GameLiftMagicOnionServer
    dotnet add package log4net
    dotnet add package websocketsharp.core
    dotnet add package Polly
    dotnet add package NewtonSoft.Json
    dotnet add package SonarAnalyzer.CSharp
  cd ..
  ```

7. Add GameLiftServer class to Server project.

  ```shell
  cat <<EOF > sandbox/GameLiftMagicOnionServer/GameLiftServer.cs
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
  EOF
  ```

8. Modify `sandbox/GameLiftMagicOnionServer/Program.cs` to use GameLiftServer.

  ```shell
  cat <<EOF > sandbox/GameLiftMagicOnionServer/GameLiftServer.cs
  using GameLiftMagicOnionServer;
  using GameLiftMagicOnionServer.Services;

  var builder = WebApplication.CreateBuilder(args);

  var gl = new GameLiftServer(builder.Configuration);
  var serverParameters = gl.InitParameters();
  gl.Start(5000, serverParameters);

  // Add services to the container.
  builder.Services.AddGrpc();
  builder.Services.AddMagicOnion();

  var app = builder.Build();

  // Configure the HTTP request pipeline.
  app.MapGrpcService<GreeterService>();
  app.MapMagicOnionService();
  app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

  app.Run();
  EOF
  ```
