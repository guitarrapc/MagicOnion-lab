using GameLiftMagicOnionServer;
using GameLiftMagicOnionServer.Services;

var builder = WebApplication.CreateBuilder(args);

using var gl = new GameLiftServer(builder.Configuration);
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
