using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Napominator;

class RabbitMQConnection
{
    string NapominatorRequestsQueue;
    string NapominatorResponseQueue ;
    
        
    string ip_last_digit;
    bool commandSent;
    bool responseReceived;
    ConnectionFactory factory;
    LogController logController;


    public RabbitMQConnection(string ip_last_digit, string ip, string username, string password, LogController logController)
    {
        commandSent = false;
        responseReceived = false;
        this.ip_last_digit = ip_last_digit;
        NapominatorResponseQueue = $"NapominatorResponseQueue_{this.ip_last_digit}";
        NapominatorRequestsQueue = "NapominatorRequestsQueue";

        factory = new ConnectionFactory()
        {
            HostName = ip,
            UserName = username,
            Password = password
        };
        this.logController = logController;
    }


    public async Task<( string, string[] )> GetConfig()
    {
        string[] lines = [];

        if (!commandSent)
        {
            commandSent = await SendCommandGetConfig();
            return ("SendCommandGetConfig", lines);
        }

        commandSent = false;
        lines = await GetResponse();

        return ("GetResponse", lines);
    }



    private async Task<string[]> GetResponse()
    {
        HashSet<string> responseSet = new HashSet<string>();
        string[] lines = [];

        try
        {
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();


            await channel.QueueDeclareAsync(
                queue: NapominatorResponseQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?> { { "x-queue-type", "quorum" } });

            while (true)
            {
                var result = await channel.BasicGetAsync(NapominatorResponseQueue, autoAck: true);
                if (result == null)
                    break;
                //await channel.BasicAckAsync(deliveryTag: result.DeliveryTag, multiple: false);
                //await channel.BasicNackAsync(result.DeliveryTag, multiple: false, requeue: true);

                var body = result.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                responseSet.Add(message);
            }


            if(responseSet.Count > 1)
            {
                logController.Log($"RabbitMQConnection.GetResponse responseSet.Count > 1  !!!!!");
            }
            foreach(var message in responseSet)
            {
                lines = message.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            }

            //logController.Log($"RabbitMQConnection.GetResponse Received Count = {responseSet.Count}");


        }
        catch (Exception e)
        {
            logController.Log($"RabbitMQConnection.GetResponse error: {e.Message}");
            return lines;
        }

        return lines;
    }
        

    private async Task<bool> SendCommandGetConfig()
    {
        try
        {
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            
            await channel.QueueDeclareAsync(
                queue: NapominatorRequestsQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?> { { "x-queue-type", "quorum" } });
            

            var props = new BasicProperties
            {
                Persistent = true,
                ContentType = "text/plain",
                Expiration = "240000", //(milliseconds) = 240 seconds 
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            string message = $"Get config for:{this.ip_last_digit}";
            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: NapominatorRequestsQueue,
                mandatory: true,
                body: body,
                basicProperties: props);

            //logController.Log($"RabbitMQConnection.SendCommandGetConfig message sent.");
        }
        catch (Exception e)
        {
            logController.Log($"RabbitMQConnection.SendCommandGetConfig error: {e.Message}");
            return false;
        }

        return true;
    }


}
