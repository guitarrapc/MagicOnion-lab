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
  gl.Start(5039, serverParameters);

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

# Create GameLift Fleet

1. Create Gamelift Fleet Anywhere to manage this machine as GameLift Fleet.

  ```shell
  # Create fleet
  aws gamelift create-location --location-name custom-location-dev-local
  aws gamelift create-fleet --name test-fleet --compute-type ANYWHERE --location "Location=custom-location-dev-local"
  ```

2. Register this machine to fleet.

  ```shell
  fleet_id=$(aws gamelift describe-fleet-attributes | jq -r '.FleetAttributes[] | select(.Name == "test-fleet") | .FleetId')
  aws gamelift register-compute --compute-name HardwareAnywhere --fleet-id "$fleet_id" --ip-address "127.0.0.1" --location custom-location-dev-local
  ```

3. Confirm your machine is registered to fleet.

  ```shell
  aws gamelift list-compute --fleet-id $fleet_id
  ```

4. Sample output.

  ```json
  {
      "ComputeList": [
          {
              "FleetId": "fleet-********************",
              "FleetArn": "arn:aws:gamelift:ap-northeast-1:*************:fleet/fleet-*************",
              "ComputeName": "HardwareAnywhere",
              "ComputeArn": "arn:aws:gamelift:ap-northeast-1:*************:compute/HardwareAnywhere",
              "IpAddress": "127.0.0.1",
              "ComputeStatus": "Active",
              "Location": "custom-location-dev-local",
              "CreationTime": "2024-03-27T18:30:45.972000+09:00",
              "GameLiftServiceSdkEndpoint": "wss://ap-northeast-1.api.amazongamelift.com"
          }
      ]
  }
  ```

# Debug with Gamelift

1. Create AuthToken for Fleet

  ```shell
  $auth_token=$(aws gamelift get-compute-auth-token --fleet-id $fleet_id --compute-name HardwareAnywhere | jq -r '.AuthToken')
  echo $auth_token
  ```

2. Sample output.

  ```json
  {
      "FleetId": "fleet-********************",
      "FleetArn": "arn:aws:gamelift:ap-northeast-1:*************:fleet/fleet-*************",
      "ComputeName": "HardwareAnywhere",
      "ComputeArn": "arn:aws:gamelift:ap-northeast-1:*************:compute/HardwareAnywhere",
      "AuthToken": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "ExpirationTimestamp": "2024-03-27T21:36:11+09:00"
  }
  ```

3. Set your Fleet Related to Environment Variables or UserSecrets.

  ```shell
  export FLEET_ID="fleet-**************************"
  export LOCATION="custom-location-dev-local"
  export HOST_ID="HardwareAnywhere"
  export AUTH_TOKEN=$auth_token
  ```

4. Run the server. Following `ProcessReady success.` message indicates that the server is ready to receive incoming game sessions. `healthcheck` message indicates that the server is healthy with GameLiftServer.

  ```shell
  $ dotnet run --project sandbox/GameLiftMagicOnionServer/GameLiftMagicOnionServer.csproj
  ProcessReady success.
  healthcheck
  info: Microsoft.Hosting.Lifetime[14]
        Now listening on: http://localhost:5039
  info: Microsoft.Hosting.Lifetime[0]
        Application started. Press Ctrl+C to shut down.
  info: Microsoft.Hosting.Lifetime[0]
        Hosting environment: Development
  info: Microsoft.Hosting.Lifetime[0]
        Content root path: C:\github\guitarrapc\MagicOnion-lab\sandbox\GameLiftMagicOnionServer
  ```

5. Create GameSession for this fleet.

  ```shell
  $ aws gamelift create-game-session --fleet-id $fleet_id --maximum-player-session-count 2 --location custom-location-dev-local
  {
      "GameSession": {
          "GameSessionId": "arn:aws:gamelift:ap-northeast-1::gamesession/fleet-**************************/custom-location-dev-local/gsess-xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
          "FleetId": "fleet-**************************",
          "FleetArn": "arn:aws:gamelift:ap-northeast-1::*************::fleet/fleet-**************************",
          "CreationTime": "2024-03-27T18:50:58.244000+09:00",
          "CurrentPlayerSessionCount": 0,
          "MaximumPlayerSessionCount": 2,
          "Status": "ACTIVATING",
          "GameProperties": [],
          "IpAddress": "127.0.0.1",
          "Port": 5039,
          "PlayerSessionCreationPolicy": "ACCEPT_ALL",
          "Location": "custom-location-dev-local"
      }
  }

  # Review GameSession with following command
  $ gamesession_id=$(aws gamelift describe-game-sessions --fleet-id $fleet_id | jq -r '.GameSessions[0].GameSessionId')
  ```

> [!TIPS]
> When activating GameSession, MagicOnionServer shows `ActivateGameSession` message as you implemented Console.WriteLine.

  ```shell
  healthcheck
  ... (repeated)
  ActivateGameSession
  healthcheck
  ```

7. Create PlayerSession to connect to Fleet, PlayerSession can create from GameSession.

  ```shell
  $ aws gamelift create-player-session --game-session-id "$gamesession_id" --player-id "player-1"
  {
      "PlayerSession": {
          "PlayerSessionId": "psess-xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
          "PlayerId": "player-1",
          "GameSessionId": "arn:aws:gamelift:ap-northeast-1::gamesession/fleet-**************************/custom-location-dev-local/gsess-xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
          "FleetId": "fleet-**************************",
          "FleetArn": "arn:aws:gamelift:ap-northeast-1::*************::fleet/fleet-**************************",
          "CreationTime": "2024-03-27T19:06:35.475000+09:00",
          "Status": "RESERVED",
          "IpAddress": "127.0.0.1",
          "Port": 5039
      }
  }
  ```

8. Use `GameSessionId`, `IpAddress` and `Port` to connect Server. Use `PlayerSesionId` to identify player.
