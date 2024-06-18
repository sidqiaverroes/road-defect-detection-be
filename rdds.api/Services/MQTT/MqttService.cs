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
using rdds.api.Services.Calculation;

namespace rdds.api.Services.MQTT
{
    public class MqttService : IHostedService, IDisposable
    {
        private readonly ILogger<MqttService> _logger;
        private readonly HiveMQClient _mqttClient;
        private readonly Dictionary<string, List<WebSocket>> _topicToWebSocketsMap;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly Dictionary<string, List<string>> _payloadBuffer = new();
        private const int MaxBufferCount = 3;
        private readonly CalculationService _calculationService;

        public MqttService(ILogger<MqttService> logger, HiveMQClient mqttClient, IServiceScopeFactory scopeFactory, CalculationService calculationService)
        {
            _logger = logger;
            _mqttClient = mqttClient;
            _topicToWebSocketsMap = new Dictionary<string, List<WebSocket>>();
            _scopeFactory = scopeFactory;
            _calculationService = calculationService;

            _mqttClient.OnMessageReceived += async (sender, args) =>
            {
                var topic = args.PublishMessage.Topic;
                var payload = args.PublishMessage.PayloadAsString;
                var topicParts = topic.Split('/');
                var deviceId = topicParts[2];  // Extract the device ID
                var attemptId = topicParts[4];  // Extract the attempt ID
                var key = $"{deviceId}/{attemptId}";
                // _logger.LogInformation($"Topic: {topic}, Received payload: {payload}");

                //Validate the topic format
                if (topicParts.Length == 5 && topicParts[0] == "rdds" && topicParts[1] == "device" && topicParts[3] == "attempt")
                {
                    // Add payload to the buffer
                    // Payload format: [{ timestamp: "YY-MM-DD hh:mm:ss.ms", latitude: xx.xxxxxx, longitude: xx.xxxxxx, velocity: xx.xx, roll: xx.xx, pitch: xx.xx, euclidean: xx,xx}]
                    lock (_payloadBuffer)
                    {
                        if (!_payloadBuffer.ContainsKey(key))
                        {
                            _payloadBuffer[key] = new List<string>();
                        }

                        // Enqueue the new payload
                        _payloadBuffer[key].Add(payload);

                        // If buffer exceeds the max count, dequeue the oldest payload
                        if (_payloadBuffer[key].Count == MaxBufferCount)
                        {
                            var payloadsToProcess = _payloadBuffer[key].ToList();
                            _payloadBuffer[key].Clear();

                            // Process the buffered payloads
                            Task.Run(async () => await ProcessPayloadsAsync(payloadsToProcess, int.Parse(attemptId), deviceId));
                        }
                    }
                    // await SendToWebSocketTopicAsync(deviceId, attemptId, payload);
                }
                else
                {
                    _logger.LogWarning($"Received payload with unmatched topic format: {topic}");
                }
            };
        }

        private async Task ProcessPayloadsAsync(List<string> payloads, int attemptId, string deviceId)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    //Dependencies
                    var _roadDataRepo = scope.ServiceProvider.GetRequiredService<IRoadDataRepository>();
                    var _calculatedDataRepo = scope.ServiceProvider.GetRequiredService<ICalculatedDataRepository>();
                    
                    //Payload preps
                    var roadDataList = new List<CalculateRoadDataDto>();
                    var payloadList = new List<SensorData>();
                    foreach (var payload in payloads)
                    {
                        var sensorDataList = JsonSerializer.Deserialize<List<SensorData>>(payload);

                        if (sensorDataList == null || sensorDataList.Count == 0)
                        {
                            throw new Exception("Failed to deserialize json");
                        }

                        //Save Road Data
                        //await _roadDataRepo.CreateFromMqttAsync(sensorDataList, attemptId);

                        // Map SensorData to CalculateRoadDataDto
                        foreach (var sensorData in sensorDataList)
                        {
                            payloadList.Add(sensorData);

                            var roadData = new CalculateRoadDataDto
                            {
                                Roll = (float)sensorData.roll,
                                Pitch = (float)sensorData.pitch,
                                Euclidean = (float)sensorData.euclidean,
                            };

                            roadDataList.Add(roadData);
                        }
                    }
                    

                    // Calculate road data to get IRI
                    int samplingFrequency = 50;
                    InternationalRoughnessIndex IRIData = CalculateIRI(roadDataList, samplingFrequency);

                    // Save road data
                    // foreach (var calculatedData in calculatedDataList)
                    // {
                    //     await _roadDataRepo.CreateFromMqttAsync(calculatedData);
                    // }

                    // // Save calculated data
                    // foreach (var calculatedData in calculatedDataList)
                    // {
                    //     await _calculatedDataRepo.SaveAsync(calculatedData);
                    // }

                    //Generate json websocket payload
                    var wsPayloads = new List<WebsocketPayload>();
                    foreach(var p in payloadList)
                    {
                        var wsp = new WebsocketPayload
                        {
                            Roll = p.roll.ToString(),
                            Pitch = p.pitch.ToString(),
                            Euclidean = p.euclidean.ToString(),
                            IRI = IRIData,
                            Velocity = p.velocity.ToString(),
                            Coordinate = new Coordinate {
                                Latitude = p.latitude,
                                Longitude = p.longitude,
                            },
                            Timestamp = p.timestamp,
                            AttemptId = attemptId.ToString()
                        };

                        wsPayloads.Add(wsp);
                    }

                    var websocketPayloads = JsonSerializer.Serialize(wsPayloads);
                    

                    //Send to Websocket
                    await SendToWebSocketTopicAsync(deviceId, attemptId.ToString(), websocketPayloads);
                    
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing payloads: {ex.Message}");
            }
        }

        private InternationalRoughnessIndex CalculateIRI(List<CalculateRoadDataDto> roadDataList, int samplingFrequency)
        {
            try{
                // Assuming roadDataList contains sequential data for a segment of time
                var rollData = roadDataList.Select(rd => (double)rd.Roll).ToArray();
                var pitchData = roadDataList.Select(rd => (double)rd.Pitch).ToArray();
                var euclideanData = roadDataList.Select(rd => (double)rd.Euclidean).ToArray();

                var (rollFrequencies, rollPsd) = _calculationService.CalculatePSD(rollData, samplingFrequency);
                var (pitchFrequencies, pitchPsd) = _calculationService.CalculatePSD(pitchData, samplingFrequency);
                var (euclideanFrequencies, euclideanPsd) = _calculationService.CalculatePSD(euclideanData, samplingFrequency);

                var iriRoll = _calculationService.CalculateIRI(rollPsd, rollFrequencies);
                var iriPitch = _calculationService.CalculateIRI(pitchPsd, pitchFrequencies);
                var iriEuclidean = _calculationService.CalculateIRI(euclideanPsd, euclideanFrequencies);
                var iriAverage = (iriRoll + iriPitch + iriEuclidean) / 3;

                var IRI = new InternationalRoughnessIndex
                            {
                                Roll = (float)iriRoll,
                                Pitch = (float)iriPitch,
                                Euclidean = (float)iriEuclidean,
                                Average = (float)iriAverage,
                                RollProfile = ClassifyRoadProfile(iriRoll),
                                PitchProfile = ClassifyRoadProfile(iriPitch),
                                EuclideanProfile = ClassifyRoadProfile(iriEuclidean),
                                AverageProfile = ClassifyRoadProfile(iriAverage),
                            };

                return IRI;
            }
            catch(Exception ex)
            {
                throw new Exception($"Failed to calculate IRI: {ex}");
            }
            
        }

        private string ClassifyRoadProfile(double iri)
        {
            if (iri <= 4)
            {
                return "Baik";
            }
            else if (iri > 4 && iri <= 8)
            {
                return "Sedang";
            }
            else if (iri > 8 && iri <= 12)
            {
                return "Rusak Ringan";
            }
            else // iri > 12
            {
                return "Rusak Berat";
            }
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
            try{
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
            catch (Exception ex)
            {
                throw new Exception($"Failed to send websocket: {ex.Message}");
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