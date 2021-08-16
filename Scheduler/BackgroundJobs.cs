using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using System;
using OpsFileTransfer.Model;
using OpsFileTransfer.Controllers;


namespace OpsFileTransfer
{

    public class BackgroundScheduler : IHostedService, IDisposable
    {
        private readonly ILogger<BackgroundScheduler> logger;
        private Timer timer;


        public BackgroundScheduler(ILogger<BackgroundScheduler> logger)
        {

            this.logger = logger;

        }

        public void Dispose()
        {
            timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            FileTransferController fileTransferController = new FileTransferController();
            this.timer = new Timer(o => fileTransferController.transferFiles(), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            return Task.CompletedTask;

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("stopping background");

            return Task.CompletedTask;
        }

    }





}