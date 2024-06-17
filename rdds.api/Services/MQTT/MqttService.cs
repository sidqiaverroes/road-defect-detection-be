using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using Microsoft.Extensions.DependencyInjection;
using rdds.api.Data;
using rdds.api.Dtos.RoadData;
using rdds.api.Interfaces;
using rdds.api.Mappers;
using rdds.api.Models;

namespace rdds.api.Services.MQTT
{
    public class MqttService : IHostedService, IDisposable
    {
        private readonly ILogger<MqttService> _logger;
        private readonly HiveMQClient _mqttClient;
        private readonly Dictionary<string, List<WebSocket>> _topicToWebSocketsMap;
        private readonly IServiceScopeFactory _scopeFactory;

        public MqttService(ILogger<MqttService> logger, HiveMQClient mqttClient, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _mqttClient = mqttClient;
            _topicToWebSocketsMap = new Dictionary<string, List<WebSocket>>();
            _scopeFactory = scopeFactory;

            _mqttClient.OnMessageReceived += async (sender, args) =>
            {
                var topic = args.PublishMessage.Topic;
                var payload = args.PublishMessage.PayloadAsString;
                var topicParts = topic.Split('/');
                var deviceId = topicParts[2];  // Extract the device ID
                var attemptId = topicParts[4];  // Extract the attempt ID
                _logger.LogInformation($"Topic: {topic}, Received payload: {payload}");
                

                    try
                    {
                        // Deserialize JSON payload into a list of SensorData
                        var sensorDataList = JsonSerializer.Deserialize<List<SensorData>>(payload);

                        if (sensorDataList == null || sensorDataList.Count == 0)
                        {
                            _logger.LogError($"Failed to deserialize payload or payload is empty: {payload}");
                            return;
                        }

                        var roadDataList = new List<CreateRoadDataDto>();

                        foreach (var sensorData in sensorDataList)
                        {

                            // Map SensorData to RoadData
                            var roadData = new CreateRoadDataDto
                            {
                                Roll = (float)sensorData.roll,
                                Pitch = (float)sensorData.pitch,
                                Euclidean = (float)sensorData.euclidean,
                                Velocity = (float)sensorData.velocity,
                                Timestamp = sensorData.timestamp,
                                Latitude = sensorData.latitude,
                                Longitude = sensorData.longitude
                            };

                            roadDataList.Add(roadData);
                        }

                        // Save RoadData to database using CreateAsync method
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var roadDataRepository = scope.ServiceProvider.GetRequiredService<IRoadDataRepository>();
                            await roadDataRepository.CreateAsync(roadDataList, int.Parse(attemptId));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing MQTT message: {ex.Message}");
                    }

                
    
                if (topicParts.Length == 5 && topicParts[0] == "rdds" && topicParts[1] == "device" && topicParts[3] == "attempt")
                {
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

            // If the websocket is not already in the list for the topic, add it
            if (!_topicToWebSocketsMap[topic].Contains(webSocket))
            {
                _topicToWebSocketsMap[topic].Add(webSocket);
                _logger.LogInformation($"WebSocket client registered for topic: {topic}");
                _logger.LogInformation($"Total clients connected: {GetTotalWebSocketCount()}");
            }
        }

        public void UnregisterWebSocket(WebSocket webSocket)
        {
            foreach (var topic in _topicToWebSocketsMap.Keys.ToList())
            {
                if (_topicToWebSocketsMap[topic].Contains(webSocket))
                {
                    _topicToWebSocketsMap[topic].Remove(webSocket);
                    _logger.LogInformation($"WebSocket client unregistered from topic: {topic}");

                    // Remove the topic if no more clients are registered
                    if (_topicToWebSocketsMap[topic].Count == 0)
                    {
                        _topicToWebSocketsMap.Remove(topic);
                        _logger.LogInformation($"Removed topic: {topic} as no clients are registered.");
                    }
                    // Do not break the loop here, as other websockets might be associated with the same topic.
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
                        break;
                    }
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        _logger.LogInformation($"Received message from WebSocket client: {message}");
                    }

                    // Clear the buffer for next read
                    Array.Clear(buffer, 0, buffer.Length);
                }
            }
            catch (WebSocketException wex) when (wex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
            {
                _logger.LogError($"WebSocket closed prematurely: {wex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"WebSocket error: {ex.Message}");
            }
            finally
            {
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