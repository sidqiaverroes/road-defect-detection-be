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
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly CalculationService _calculationService;
        private readonly Dictionary<string, List<WebSocket>> _topicToWebSocketsMap;
        private readonly Dictionary<string, List<string>> _payloadBuffer = new Dictionary<string, List<string>>();
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private const int MaxBufferCount = 3;
        private bool _handlingStartMessage = false;

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

                // Validate the topic format
                if (topicParts.Length == 3 && topicParts[0] == "rdds" && topicParts[1] == "device")
                {
                    // Check if the device is registered
                    var deviceMac = await CheckDeviceRegisteredAsync(deviceId);
                    if (deviceMac == null)
                    {
                        _logger.LogWarning($"Device ID {deviceId} is not registered.");
                        return;
                    }
                    
                    // Handle "Start", regular payloads, and "End"
                    if (payload == "Start")
                    {
                        await HandleStartMessage(deviceMac);
                        _logger.LogWarning($"Received payload START");
                    }
                    else if (payload == "End")
                    {
                        await HandleEndMessage(deviceMac);
                    }
                    else
                    {
                        await WaitForStartMessageCompletion();
                        await HandleRegularPayload(key, payload, deviceMac);
                        _logger.LogWarning($"Received payload REGULAR");
                    }
                }
                else
                {
                    _logger.LogWarning($"Received payload with unmatched topic format: {topic}");
                }
            };
        }

        private async Task WaitForStartMessageCompletion()
        {
            // Wait until HandleStartMessage completes
            while (_handlingStartMessage)
            {
                await Task.Delay(100); // Adjust delay time as needed
            }
        }

        private async Task<string?> CheckDeviceRegisteredAsync(string deviceId)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var deviceRepo = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();
                return await deviceRepo.IsExistedAsync(deviceId);
            }
        }

        private async Task HandleStartMessage(string deviceMac)
        {
            _logger.LogWarning($"handling START");
            var uniqueId = Guid.NewGuid();
            // _logger.LogInformation($"[{DateTime.Now:O}] [{uniqueId}] Handling start message for device {deviceMac}");
            _handlingStartMessage = true;
            await _semaphore.WaitAsync();

            try
            {
                // _logger.LogInformation($"[{DateTime.Now:O}] [{uniqueId}] Entered lock for device {deviceMac}");
            
                using (var scope = _scopeFactory.CreateScope())
                {
                    var attemptRepo = scope.ServiceProvider.GetRequiredService<IAttemptRepository>();

                    // Check if the last attempt is finished
                    var lastAttempt = await attemptRepo.GetLastAttempt(deviceMac);
                    if (lastAttempt != null && !lastAttempt.IsFinished)
                    {
                        _logger.LogWarning($"[{DateTime.Now:O}] [{uniqueId}] Last attempt for device {deviceMac} is not finished yet.");
                        return;
                    }

                    // Create a new attempt
                    var attemptDto = new CreateAttemptDto
                    {
                        Title = "title",
                        Description = "desc",
                    };
                    var attemptModel = attemptDto.ToAttemptFromCreate(deviceMac);
                    var newAttempt = await attemptRepo.CreateAsync(attemptModel, deviceMac);
                    if (newAttempt == null)
                    {
                        _logger.LogWarning($"[{DateTime.Now:O}] [{uniqueId}] Failed to create a new attempt for device {deviceMac}");
                    }
                    else
                    {
                        _logger.LogInformation($"[{DateTime.Now:O}] [{uniqueId}] Created new attempt for device {deviceMac}");
                    }
                }
            }
            finally
            {
                _semaphore.Release();
                _handlingStartMessage = false;
                _logger.LogWarning($"handling START COMPLETE");
                // _logger.LogInformation($"[{DateTime.Now:O}] [{uniqueId}] Exiting lock for device {deviceMac}");
            }
        }


        private async Task HandleEndMessage(string deviceMac)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var attemptRepo = scope.ServiceProvider.GetRequiredService<IAttemptRepository>();
                var lastAttempt = await attemptRepo.GetLastAttempt(deviceMac);
                if (lastAttempt == null)
                {
                    _logger.LogWarning($"No ongoing attempt found for device {deviceMac}");
                }
                else
                {
                    await attemptRepo.FinishAsync(lastAttempt.Id);
                    _logger.LogInformation($"Finished attempt {lastAttempt.Id} for device {deviceMac}");
                }
            }
        }

        private async Task HandleRegularPayload(string key, string payload, string deviceId)
        {
            _logger.LogWarning($"handling REG");
            using (var scope = _scopeFactory.CreateScope())
            {
                var attemptRepo = scope.ServiceProvider.GetRequiredService<IAttemptRepository>();
                var lastAttempt = await attemptRepo.GetLastAttempt(deviceId);

                if(lastAttempt == null)
                {
                    throw new Exception("no last attempt");
                }

                var attemptId = lastAttempt.Id;

                // Add payload to the buffer
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
                        Task.Run(async () => await ProcessPayloadsAsync(payloadsToProcess, attemptId, deviceId));
                    }
                }
            }
        }

        private async Task ProcessPayloadsAsync(List<string> payloads, int attemptId, string deviceId)
        {
            if(attemptId <= 0)
            {
                throw new Exception("Invalid attemptId");
            }
            await _semaphore.WaitAsync();
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
                    
                     _logger.LogWarning($"--- processing: {roadDataList.Count}");
                    // Calculate road data to get IRI
                    int samplingFrequency = 50;
                    InternationalRoughnessIndex IRIData = CalculateIRI(roadDataList, samplingFrequency);

                    // Save road data
                    await _roadDataRepo.CreateFromMqttAsync(sensorDataList, attemptId);

                    // Save calculated data
                    await _calculatedDataRepo.CreateFromMqttAsync(sensorDataList, IRIData, attemptId);

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
            finally
            {
                _semaphore.Release();
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
                // _logger.LogError($"PSD DATAAAAA: {rollFrequencies.Length}, {rollPsd.Length}");
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

                var topic = "rdds/device/+";
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