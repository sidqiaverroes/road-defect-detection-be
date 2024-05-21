using System;
using System.Threading.Tasks;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.Types;

class Program
{
    static async Task Main(string[] args)
    {
        var options = new HiveMQClientOptions
        {
            Host = "localhost",
            Port = 1883,
            CleanStart = false,  // Set to false to receive messages queued on the broker
            ClientId = "ConnectReceiveAndPublish",
        };

        var client = new HiveMQClient(options);

        // Message Handler
        client.OnMessageReceived += (sender, args) =>
        {
            var jsonString = args.PublishMessage.PayloadAsString;
            Console.WriteLine($"Received message: {jsonString}");
        };

        // Connect to the broker
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        if (connectResult.ReasonCode != HiveMQtt.MQTT5.ReasonCodes.ConnAckReasonCode.Success)
        {
            throw new Exception($"Failed to connect: {connectResult.ReasonString}");
        }

        // Subscribe to a topic
        var topic = "hivemqtt/sendmessageonloop";
        var subscribeResult = await client.SubscribeAsync(topic, QualityOfService.AtLeastOnceDelivery).ConfigureAwait(false);
        Console.WriteLine($"Subscribed to {topic}: {subscribeResult.Subscriptions[0].SubscribeReasonCode}");

        // Set up a task to disconnect after 5 minutes
        var disconnectTask = Task.Delay(TimeSpan.FromMinutes(5));

        // Wait for either 5 minutes to elapse or for the user to request disconnection
        await Task.WhenAny(disconnectTask);
        
        Console.WriteLine("Disconnecting gracefully...");
        await client.DisconnectAsync().ConfigureAwait(false);
    }
}
