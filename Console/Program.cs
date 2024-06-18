using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using HiveMQtt.MQTT5.Types;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class Program
{
    public class SensorData
    {
        public string timestamp { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public double velocity { get; set; }
        public double roll { get; set; }
        public double pitch { get; set; }
        public double euclidean { get; set; }
    }

    public static async Task Main()
    {
        var options = new HiveMQClientOptions
        {
            Host = "localhost",
            Port = 1883,
            UserName = "admin",
            Password = "hivemq",
        };

        var client = new HiveMQClient(options);

        // Message Handler
        client.OnMessageReceived += (sender, args) =>
        {
            var jsonString = args.PublishMessage.PayloadAsString;
            var jsonDocument = JsonDocument.Parse(jsonString);

            Console.WriteLine($"Message Received; topic={args.PublishMessage.Topic}, payload={jsonString}");
        };

        // Connect to the broker
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        if (connectResult.ReasonCode != HiveMQtt.MQTT5.ReasonCodes.ConnAckReasonCode.Success)
        {
            throw new Exception($"Failed to connect: {connectResult.ReasonString}");
        }

        var message_number = 0;
        while (true)
        {
            message_number++;
            // Generate sensor data array
            var sensorDataList = new List<SensorData>();
            var random = new Random();

            for (int i = 0; i < 5; i++)
            {
                var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ff");
                var latitude = random.NextDouble() * 180 - 90;   // Example random latitude within [-90, 90]
                var longitude = random.NextDouble() * 360 - 180; // Example random longitude within [-180, 180]
                var velocity = random.NextDouble() * 100;        // Example random velocity within [0, 100]
                var roll = random.NextDouble() * 180 - 90;       // Example random roll within [-90, 90]
                var pitch = random.NextDouble() * 180 - 90;      // Example random pitch within [-90, 90]
                var euclidean = random.NextDouble() * 100;       // Example random euclidean within [0, 100]

                var data = new SensorData
                {
                    timestamp = timestamp,
                    latitude = latitude,
                    longitude = longitude,
                    velocity = velocity,
                    roll = roll,
                    pitch = pitch,
                    euclidean = euclidean
                };

                sensorDataList.Add(data);
            }
        
            // Serialize sensor data array to JSON
            var json = JsonSerializer.Serialize(sensorDataList);

            // Publish JSON array to MQTT topic
            var topic = "rdds/device/08:D1:F9:E1:A2:34/attempt/1";
            var message = new MQTT5PublishMessage
            {
                Topic = topic,
                Payload = Encoding.UTF8.GetBytes(json), // Serialize JSON to UTF-8 bytes
                QoS = QualityOfService.AtLeastOnceDelivery,
            };

            var resultPublish = await client.PublishAsync(message).ConfigureAwait(false);
            Console.WriteLine($"Published {sensorDataList.Count} messages to topic {topic}: {resultPublish.QoS1ReasonCode}");

            if(message_number == 5000){
                Console.WriteLine("Disconnecting gracefully...");
                await client.DisconnectAsync().ConfigureAwait(false);
                return;
            }

            await Task.Delay(1000).ConfigureAwait(false);
        }
    }
}
