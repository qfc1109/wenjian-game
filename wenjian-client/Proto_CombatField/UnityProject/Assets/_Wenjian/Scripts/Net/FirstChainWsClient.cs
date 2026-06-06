using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Wenjian.Client.Net
{
    public sealed class FirstChainWsClient : MonoBehaviour
    {
        public const string DefaultEndpoint = "ws://127.0.0.1:18080/ws/first-chain";

        [SerializeField]
        private string endpoint = DefaultEndpoint;

        [SerializeField]
        private string loginKey = "local-dev-key";

        [SerializeField]
        private bool autoConnectOnStart = true;

        [SerializeField]
        private FirstChainHudController hudController;

        private CancellationTokenSource cancellation;

        public string Endpoint
        {
            get => endpoint;
            set => endpoint = string.IsNullOrWhiteSpace(value) ? DefaultEndpoint : value;
        }

        public string LoginKey
        {
            get => loginKey;
            set => loginKey = string.IsNullOrWhiteSpace(value) ? "local-dev-key" : value;
        }

        public bool AutoConnectOnStart
        {
            get => autoConnectOnStart;
            set => autoConnectOnStart = value;
        }

        public FirstChainHudController HudController
        {
            get => hudController;
            set => hudController = value;
        }

        private async void Start()
        {
            if (!autoConnectOnStart)
            {
                return;
            }

            cancellation = new CancellationTokenSource();
            await ConnectAndRunAsync(cancellation.Token);
        }

        private void OnDestroy()
        {
            cancellation?.Cancel();
            cancellation?.Dispose();
            cancellation = null;
        }

        public async Task ConnectAndRunAsync(CancellationToken cancellationToken)
        {
            if (hudController == null)
            {
                return;
            }

            try
            {
                using var socket = new ClientWebSocket();
                await socket.ConnectAsync(new Uri(endpoint), cancellationToken);
                await SendTextAsync(socket, FirstChainCommandBuilder.BuildLogin(loginKey, CurrentTimeMillis()), cancellationToken);

                while (socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    string payload = await ReceiveTextAsync(socket, cancellationToken);
                    if (payload == null)
                    {
                        break;
                    }

                    hudController.ApplyServerPayload(payload);
                    while (hudController.TryDequeueOutgoingCommand(out string command))
                    {
                        await SendTextAsync(socket, command, cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                hudController.ApplyConnectionFailure("Disconnected");
            }
            catch (Exception exception)
            {
                hudController.ApplyConnectionFailure($"Connect failed: {exception.GetType().Name}");
            }
        }

        private static long CurrentTimeMillis()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        private static Task SendTextAsync(ClientWebSocket socket, string text, CancellationToken cancellationToken)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            return socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken);
        }

        private static async Task<string> ReceiveTextAsync(ClientWebSocket socket, CancellationToken cancellationToken)
        {
            var buffer = new byte[4096];
            using var stream = new MemoryStream();
            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    return null;
                }

                stream.Write(buffer, 0, result.Count);
            }
            while (!result.EndOfMessage);

            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}
