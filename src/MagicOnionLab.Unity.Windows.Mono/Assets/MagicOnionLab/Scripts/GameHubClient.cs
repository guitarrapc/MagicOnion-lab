using Grpc.Core;
using MagicOnion.Client;
using MagicOnionLab.Shared.Hubs;
using MagicOnionLab.Shared.Mpos;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MagicOnionLab.Unity
{
    public class GameHubClient : IGameHubReceiver, IAsyncDisposable
    {
        private ChannelBase _channel;
        private IGameHub _client;
        private readonly ILogger _logger;
        private readonly string _userName;
        private int _index;
        private bool _isSelfDisConnected;

        public GameHubClient(string userName, int index)
        {
            _logger = new UnityCustomLogger(true);
            _userName = userName;
            _index = index;
        }

        public async ValueTask ConnectAsync(ChannelBase channel, string roomName, int capacity, CancellationToken ct)
        {
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
                _logger.LogInformation($"disconnected from the server.");

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
            _logger.LogInformation($"Reconnecting to the server...");
            _client = await StreamingHubClient.ConnectAsync<IGameHub, IGameHubReceiver>(_channel, this);
            RegisterDisconnect(_client).FireAndForget();
            _logger.LogInformation("Reconnected.");

            _isSelfDisConnected = false;
        }


        public async ValueTask ReadyAsync()
        {
            if (_client is null) throw new ArgumentNullException(nameof(_client));

            // ready
            _logger.LogInformation($"Ready matching: {_userName}");
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
            _logger.LogInformation($"Create room: {roomName}");
        }

        public void OnJoinRoom(string userName)
        {
            if (_userName.Equals(userName, StringComparison.Ordinal))
            {
                _logger.LogInformation($"Join user: {userName}");
            }
        }

        public void OnLeaveRoom(string userName)
        {
            if (_userName.Equals(userName, StringComparison.Ordinal))
            {
                _logger.LogInformation($"Leave user: {userName}");
            }
        }

        public void OnMatchCompleted()
        {
            _logger.LogInformation($"Matching complete.");
        }

        public void OnUpdateUserInfo(GameRoomUserInfoUpdateResponse response)
        {
            if (_userName.Equals(response.UserName, StringComparison.Ordinal))
            {
                _logger.LogInformation($"Update UserInfo: userName {response.UserName}, position: ({response.Position.x},{response.Position.y},{response.Position.z})");
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
