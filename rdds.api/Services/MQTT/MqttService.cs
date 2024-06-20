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
using rdds.api.Dtos.Attempt;
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
                var key = deviceId;
                int attemptId = 0;
                // _logger.LogInformation($"Topic: {topic}, Received payload: {payload}");

                //Validate the topic format
                if (topicParts.Length == 3 && topicParts[0] == "rdds" && topicParts[1] == "device")
                {
                    bool shouldProcessStart = false;
                    bool shouldProcessEnd = false;
                    List<string> payloadsToProcess = null;

                    lock (_payloadBuffer)
                    {
                        if (!_payloadBuffer.ContainsKey(key))
                        {
                            _payloadBuffer[key] = new List<string>();
                        }

                        // Add payload to the buffer
                        _payloadBuffer[key].Add(payload);

                        // Check for "Start" or "End" payload
                        if (payload == "Start")
                        {
                            shouldProcessStart = true;
                        }
                        else if (payload == "End")
                        {
                            shouldProcessEnd = true;
                        }
                        else if (_payloadBuffer[key].Count == MaxBufferCount + 1) // Ensure we have enough data for at least 3 batches
                        {
                            payloadsToProcess = ExtractRegularPayloads(key);
                        }
                    }

                    if (shouldProcessStart)
                    {
                        try
                        {
                            attemptId = await ProcessStartAsync(deviceId);
                            _logger.LogInformation($"Processed 'Start' message for device {deviceId}, new attemptId: {attemptId}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error processing 'Start' message for device {deviceId}: {ex.Message}");
                        }
                    }

                    if (shouldProcessEnd)
                    {
                        _logger.LogInformation($"Received 'End' message for device {deviceId}");
                    }

                    if (payloadsToProcess != null)
                    {
                        try
                        {
                            await ProcessPayloadsAsync(payloadsToProcess, attemptId, deviceId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error processing regular payloads for device {deviceId}: {ex.Message}");
                        }
                    }
                }
                else
                {
                    _logger.LogWarning($"Received payload with unmatched topic format: {topic}");
                }
            };
        }

        private async Task<int> ProcessStartAsync(string deviceMac)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                //Dependencies
                var _attemptRepo = scope.ServiceProvider.GetRequiredService<IAttemptRepository>();
                var _deviceRepo = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();

                //is the device registered? continue : throw
                var existedDeviceMac = await _deviceRepo.IsExistedAsync(deviceMac);

                if(existedDeviceMac == null)
                {
                    throw new Exception("Device is not Registered");
                }

                var attemptDto = new CreateAttemptDto
                {
                    Title = "title",
                    Description = "desc",
                };

                var attemptModel = attemptDto.ToAttemptFromCreate(deviceMac);
                
                var createdAttempt = await _attemptRepo.CreateAsync(attemptModel);

                if(createdAttempt == null)
                {
                    throw new Exception("There is still ongoing attempt");
                }

                int attemptId = createdAttempt.Id;

                return attemptId;
            }
        }

        private async Task ProcessEndAsync(string deviceMac)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                //Dependencies
                var _attemptRepo = scope.ServiceProvider.GetRequiredService<IAttemptRepository>();
                var _deviceRepo = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();

                
            }
        }

        private List<string> ExtractRegularPayloads(string key)
        {
            var payloadsToProcess = new List<string>();

            // Extract batches of regular payloads (excluding "Start" and "End")
            var allPayloads = _payloadBuffer[key];
            var startIndex = 1;
            var batch = allPayloads.Skip(startIndex).Take(MaxBufferCount).ToList();
            payloadsToProcess.AddRange(batch);

            // Remove processed payloads from the buffer
            _payloadBuffer[key].RemoveRange(startIndex, MaxBufferCount);

            return payloadsToProcess;
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
                    var sensorDataList = new List<SensorData>();
                    foreach (var payload in payloads)
                    {
                        var _sensorDataList = JsonSerializer.Deserialize<List<SensorData>>(payload);

                        if (_sensorDataList == null || _sensorDataList.Count == 0)
                        {
                            throw new Exception("Failed to deserialize json");
                        }

                        // Map SensorData to CalculateRoadDataDto
                        foreach (var sensorData in _sensorDataList)
                        {
                            sensorDataList.Add(sensorData);

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

                    // // Save road data
                    // await _roadDataRepo.CreateFromMqttAsync(sensorDataList, attemptId);

                    // // Save calculated data
                    // await _calculatedDataRepo.CreateFromMqttAsync(sensorDataList, IRIData, attemptId);

                    //Generate json websocket payload
                    var payloadList = new List<WebsocketPayload>();
                    foreach(var p in sensorDataList)
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

                        payloadList.Add(wsp);
                    }

                    var websocketPayload = JsonSerializer.Serialize(payloadList);
                    

                    //Send to Websocket
                    await SendToWebSocketTopicAsync(deviceId, attemptId.ToString(), websocketPayload);
                    
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