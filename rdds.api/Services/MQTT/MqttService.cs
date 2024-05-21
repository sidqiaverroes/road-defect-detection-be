using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using rdds.api.Data;

namespace rdds.api.Services.MQTT
{
    public class MqttService : IHostedService, IDisposable
    {
        private readonly ILogger<MqttService> _logger;
        private readonly HiveMQClient _mqttClient;
        private readonly Dictionary<string, List<WebSocket>> _topicToWebSocketsMap;

        public MqttService(ILogger<MqttService> logger, HiveMQClient mqttClient)
        {
            _logger = logger;
            _mqttClient = mqttClient;
            _topicToWebSocketsMap = new Dictionary<string, List<WebSocket>>();

            _mqttClient.OnMessageReceived += async (sender, args) =>
            {
                var topic = args.PublishMessage.Topic;
                var payload = args.PublishMessage.PayloadAsString;
                _logger.LogInformation($"Topic: {topic}, Received payload: {payload}");

                var topicParts = topic.Split('/');
    
                if (topicParts.Length == 5 && topicParts[0] == "rdds" && topicParts[1] == "device" && topicParts[3] == "attempt")
                {
                    var deviceId = topicParts[2];  // Extract the device ID
                    var attemptId = topicParts[4];  // Extract the attempt ID
                    await SendToWebSocketTopicAsync(deviceId, attemptId, payload);
                }
                else
                {
                    _logger.LogWarning($"Received payload with unmatched topic format: {topic}");
                }
            };
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var connectResult = await _mqttClient.ConnectAsync().ConfigureAwait(false);
                if (connectResult.ReasonCode != HiveMQtt.MQTT5.ReasonCodes.ConnAckReasonCode.Success)
                {
                    _logger.LogError($"Failed to connect: {connectResult.ReasonString}");
                    throw new Exception($"Failed to connect: {connectResult.ReasonString}");
                }

                var topic = "rdds/device/+/attempt/+";
                await _mqttClient.SubscribeAsync(topic, HiveMQtt.MQTT5.Types.QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
                _logger.LogInformation($"Subscribed to {topic}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception during MQTT connection: {ex.Message}");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _mqttClient.DisconnectAsync().ConfigureAwait(false);
            _logger.LogInformation("Disconnected gracefully from MQTT broker.");
        }

        public void RegisterWebSocketForTopic(WebSocket webSocket, string topic)
        {
            if (!_topicToWebSocketsMap.ContainsKey(topic))
            {
                _topicToWebSocketsMap[topic] = new List<WebSocket>();
            }
            _topicToWebSocketsMap[topic].Add(webSocket);
            _logger.LogInformation($"WebSocket client registered for topic: {topic}");
            _logger.LogInformation($"Total clients connected: {GetTotalWebSocketCount()}");
        }

        public void UnregisterWebSocket(WebSocket webSocket)
        {
            foreach (var topic in _topicToWebSocketsMap.Keys.ToList())
            {
                if (_topicToWebSocketsMap[topic].Contains(webSocket))
                {
                    _topicToWebSocketsMap[topic].Remove(webSocket);
                    _logger.LogInformation($"WebSocket client unregistered from topic: {topic}");

                    if (_topicToWebSocketsMap[topic].Count == 0)
                    {
                        _topicToWebSocketsMap.Remove(topic);
                        _logger.LogInformation($"Removed topic: {topic} as no clients are registered.");
                    }
                    break;
                }
            }
        }

        public async Task SendToWebSocketTopicAsync(string deviceId, string attemptId, string payload)
        {
            var topic = $"{deviceId}/{attemptId}";

            if (_topicToWebSocketsMap.TryGetValue(topic, out var webSockets))
            {
                foreach (var webSocket in webSockets)
                {
                    await SendToWebSocketAsync(webSocket, payload);
                }
            }
            else
            {
                _logger.LogWarning($"No WebSocket clients registered for topic: {topic}");
            }
        }

        private async Task SendToWebSocketAsync(WebSocket webSocket, string message)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                var buffer = Encoding.UTF8.GetBytes(message);
                await webSocket.SendAsync(new ArraySegment<byte>(buffer),
                                          WebSocketMessageType.Text,
                                          true,
                                          CancellationToken.None);
                _logger.LogInformation($"Sent message to WebSocket client: {message}");
            }
        }

        public async Task HandleWebSocketAsync(WebSocket webSocket, string deviceId, string attemptId)
        {
            var topic = $"{deviceId}/{attemptId}";
            RegisterWebSocketForTopic(webSocket, topic);
            
            var buffer = new byte[1024 * 4];
            var pingInterval = TimeSpan.FromMinutes(1); // Ping interval

            using var pingCancellationTokenSource = new CancellationTokenSource();
            var pingTask = Task.Run(async () =>
            {
                try
                {
                    while (webSocket.State == WebSocketState.Open)
                    {
                        var pingMessage = Encoding.UTF8.GetBytes("{\"type\":\"ping\"}");
                        await webSocket.SendAsync(new ArraySegment<byte>(pingMessage),
                                                    WebSocketMessageType.Text,
                                                    true,
                                                    CancellationToken.None);
                        _logger.LogInformation("Sent ping to WebSocket client");
                        await Task.Delay(pingInterval, pingCancellationTokenSource.Token);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error sending ping: {ex.Message}");
                }
            });

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        UnregisterWebSocket(webSocket);
                        _logger.LogInformation("WebSocket connection closed");
                    }
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        _logger.LogInformation($"Received message from WebSocket client: {message}");

                        // Handle incoming message (e.g., pong)
                        if (message == "{\"type\":\"pong\"}")
                        {
                            _logger.LogInformation("Received pong from client");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"WebSocket error: {ex.Message}");
            }
            finally
            {
                pingCancellationTokenSource.Cancel(); // Stop the ping task
                await pingTask; // Wait for the ping task to complete
                UnregisterWebSocket(webSocket);
                _logger.LogInformation($"Client disconnected. Total connected clients: {GetTotalWebSocketCount()}");
            }
        }



        public int GetTotalWebSocketCount()
        {
            return _topicToWebSocketsMap.Sum(x => x.Value.Count);
        }
        
        public void Dispose()
        {
            try
            {
                _mqttClient?.Dispose();
            }
            catch (ObjectDisposedException)
            {
                // Ignore the exception if _mqttClient is already disposed
            }
        }
    }
}