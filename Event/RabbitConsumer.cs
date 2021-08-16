using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpsFileTransfer.Model;
using Newtonsoft.Json;
using System.IO;


namespace OpsFileTransfer
{
    public static class OpsFTRabbitConsumer
    {
        public static void ActivityConsume(IModel channel)
        {
            channel.ExchangeDeclare(GlobalObjects._rabbitFIExchange, ExchangeType.Direct, durable: true);
            channel.QueueDeclare(GlobalObjects._rabbitActivityQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            channel.QueueBind(GlobalObjects._rabbitActivityQueue, GlobalObjects._rabbitFIExchange, GlobalObjects._DEFAULTROUTE);//Using default for activities
            channel.BasicQos(0, 1, false);
            Console.WriteLine("Consumer started");


            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, e) =>
            {
                var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                using (TextReader sr = new StringReader(message))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    RabbitMessage rabbitMessage = (RabbitMessage)serializer.Deserialize(sr, typeof(RabbitMessage));

                    switch (rabbitMessage.serviceId)
                    {

                        case "24GT":
                            TwoFourToGTM twoFourToGTM = new TwoFourToGTM();
                            twoFourToGTM.processService(rabbitMessage.transferId, rabbitMessage.fileTransferDefinition, rabbitMessage.serviceId, rabbitMessage.transferFileName, rabbitMessage.tmpFolder);
                            break;

                        case "END":
                            OpsITEndService opsITEndService = new OpsITEndService();
                            opsITEndService.EndService(rabbitMessage.transferId, rabbitMessage.fileTransferDefinition, rabbitMessage.serviceId, rabbitMessage.transferFileName, rabbitMessage.tmpFolder);
                            break;

                        default:
                            Console.WriteLine("NO Handler found for Transfer type " + rabbitMessage.fileTransferDefinition.transfertypeid);
                            break;
                    }


                }

            };
            channel.BasicConsume(GlobalObjects._rabbitActivityQueue, true, consumer);
            Console.ReadLine();
        }
    }

}