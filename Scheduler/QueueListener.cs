
using System;
using System.IO;
using OpsFileTransfer.Model;
using OpsFileTransfer.DAL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Security.Permissions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpsFileTransfer
{

    public class OpsFTQueueListener : IHostedService
    {

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory() { HostName = GlobalObjects._rabbitHost };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            Console.WriteLine("Now in background task of consume Start Queue");
            OpsFTRabbitConsumer.ActivityConsume(channel);

            return Task.CompletedTask;

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {

            return Task.CompletedTask;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }



}
