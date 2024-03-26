using Grpc.Core;
using MagicOnion.Client;
using MagicOnionLab.Shared.Hubs;
using MagicOnionLab.Shared.Mpos;
using MagicOnionLab.Unity.Infrastructures.Loggers;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MagicOnionLab.Unity.Hubs
{
    public class GameHubClient : IGameHubReceiver, IAsyncDisposable
    {
        private ChannelBase _channel;
        private IGameHub _client;
        private readonly ILogger _logger;
        private string _roomName;
        private readonly string _userName;
        private int _index;
        private Action<string> _onReceive;
        private bool _isSelfDisConnected;

        public GameHubClient(ILogger logger, Action<string> onReceive, string userName, int index)
        {
            _logger = logger;
            _userName = userName;
            _index = index;
            _onReceive = onReceive;
        }

        public async ValueTask ConnectAsync(ChannelBase channel, string roomName, int capacity, CancellationToken ct)
        {
            _roomName = roomName;
            _channel = channel;
            _client = await StreamingHubClient.ConnectAsync<IGameHub, IGameHubReceiver>(channel, this, cancellationToken: ct);
            RegisterDisconnect(_client).FireAndForget();

            // create
            await _client.CreateRoomAsync(new GameRoomCreateRequest
            {
                RoomName = roomName,
                Capacity = capacity,
            });

            // join
            await _client.JoinRoomAsync(new GameRoomJoinRequest
            {
                RoomName = roomName,
                UserName = _userName,
            });
        }

        public async Task RegisterDisconnect(IGameHub client)
        {
            try
            {
                // you can wait disconnected event
                await client.WaitForDisconnect();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            finally
            {
                // try-to-reconnect? logging event? close? etc...
                _logger.LogInformation($"disconnected from the server. (Room: {_roomName})");

                if (_isSelfDisConnected)
                {
                    // there is no particular meaning
                    await Task.Delay(2000);

                    // reconnect
                    await ReconnectServerAsync();
                }
            }
        }

        private async Task ReconnectServerAsync()
        {
            _logger.LogInformation($"Reconnecting to the server... (Room: {_roomName})");
            _client = await StreamingHubClient.ConnectAsync<IGameHub, IGameHubReceiver>(_channel, this);
            RegisterDisconnect(_client).FireAndForget();
            _logger.LogInformation($"Reconnected. (Room: {_roomName})");

            _isSelfDisConnected = false;
        }


        public async ValueTask ReadyAsync()
        {
            if (_client is null) throw new ArgumentNullException(nameof(_client));

            // ready
            _logger.LogInformation($"Ready matching. (Room: {_roomName})");
            await _client.ReadyMatchAsync();
        }

        public async ValueTask UpdateUserInfoAsync()
        {
            if (_client is null) throw new ArgumentNullException(nameof(_client));

            // update
            for (var i = 0; i < 10; i++)
            {
                var position = i * (_index + 1);
                await _client.UpdateUserInfonAsync(new GameRoomUserInfoUpdateRequest
                {
                    Position = new Vector3(position, position, position),
                });
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        public async ValueTask LeaveAsync()
        {
            if (_client is null) throw new ArgumentNullException(nameof(_client));
            await _client.LeaveRoomAsync();
            _isSelfDisConnected = true;
        }

        public void OnCreateRoom(string roomName)
        {
            var log = $"Create room: {roomName}";
            _logger.LogInformation(log);
            _onReceive(log);
        }

        public void OnJoinRoom(string userName)
        {
            if (_userName.Equals(userName, StringComparison.Ordinal))
            {
                var log = $"Join user: {userName} (Room: {_roomName})";
                _logger.LogInformation(log);
                _onReceive(log);
            }
        }

        public void OnLeaveRoom(string userName)
        {
            if (_userName.Equals(userName, StringComparison.Ordinal))
            {
                var log = $"Leave user: {userName} (Room: {_roomName})";
                _logger.LogInformation(log);
                _onReceive(log);
            }
        }

        public void OnMatchCompleted()
        {
            var log = $"Matching complete. (Room: {_roomName})";
            _logger.LogInformation(log);
            _onReceive(log);
        }

        public void OnUpdateUserInfo(GameRoomUserInfoUpdateResponse response)
        {
            if (_userName.Equals(response.UserName, StringComparison.Ordinal))
            {
                var log = $"Update UserInfo: userName {response.UserName}, position: {{{response.Position.x},{response.Position.y},{response.Position.z}}} (Room: {_roomName})";
                _logger.LogInformation(log);
                _onReceive(log);
            }
        }

        public Task DisposeAsync()
        {
            if (_client is not null)
            {
                return _client.DisposeAsync();
            }
            return Task.CompletedTask;
        }

        // You can watch connection state, use this for retry etc.
        public Task WaitForDisconnect()
        {
            if (_client is not null)
            {
                return _client.WaitForDisconnect();
            }
            return Task.CompletedTask;
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (_client is not null)
            {
                await _client.DisposeAsync();
            }
        }
    }
}
