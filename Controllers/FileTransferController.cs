using System;
using System.IO;
using OpsFileTransfer.Model;
using OpsFileTransfer.DAL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Security.Permissions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Collections.Generic;

namespace OpsFileTransfer.Controllers
{
    public class FileTransferController
    {

        public static string _tdfilename;
        public static string _connectionString;

        public OpsFTControl oFTControl { get; set; }

        public FileTransferController()
        {


            this.oFTControl = GlobalObjects._oFTControl;
            _connectionString = GlobalObjects._connectionString;
            _tdfilename = GlobalObjects._tdfilename;


        }

        public void transferFiles()
        {
            for (int i = 0; i < oFTControl.oftcontrol.Count; i++)
            {

                FileTransferDefinition fileTransferDefinition = oFTControl.oftcontrol[i];
                if (!startTransferService(fileTransferDefinition))
                {

                    Console.WriteLine("Skipping some files for transfer definition");

                }

            }
        }

        public bool startTransferService(FileTransferDefinition fileTransferDefinition)
        {
            int fiflog_key = 0;
            var factory = new ConnectionFactory() { HostName = GlobalObjects._rabbitHost };
            int fileCounter = 0;
            foreach (string file1 in Directory.EnumerateFiles(fileTransferDefinition.source.folder, fileTransferDefinition.source.filepattern))
            {

                //Safety valve - dont process too many in one go leading to threading issues.
                if (fileCounter > 20)
                {
                    break;
                }

                var fiflogdal = new FiflogDAL(_connectionString);
                fiflog_key = fiflogdal.StartFiflog(file1, fileTransferDefinition.transfertypeid);
                if (fiflog_key <= 0)
                {
                    Console.WriteLine("Unable to Insert Start record");
                    throw new Exception("error in inserting to database the start step");
                    return false;
                }
                string nameOfFile = "";
                if (Directory.Exists(fileTransferDefinition.others.tmpfolder))
                {
                    FileInfo fileInfo = new FileInfo(file1);
                    nameOfFile = fileInfo.Name;
                    try
                    {
                        //fileInfo.Open(fileInfo.Name, FileMode.Open, FileAccess.Read, FileShare.None);
                        fileInfo.MoveTo($@"{fileTransferDefinition.others.tmpfolder}/{fileInfo.Name}");
                        string destinationFileName = fileTransferDefinition.others.tmpfolder + "/" + fileInfo.Name;
                        FileInfo fileInfo1 = new FileInfo(destinationFileName);
                        if (!fileInfo1.Exists)
                        {
                            Console.WriteLine("Keep Waiting till transfer complete");

                        }
                        //fileInfo.MoveTo(fileTransferDefinition.others.tmpfolder);

                    }
                    catch
                    {
                        Console.WriteLine("Unable to move file " + nameOfFile);
                        return false;
                    }


                }
                //Publish to FIEXCHANGE for pickup
                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.ExchangeDeclare(exchange: GlobalObjects._rabbitFIExchange, type: ExchangeType.Direct, durable: true);
                        RabbitMessage rabbitMessage = new RabbitMessage();
                        rabbitMessage.transferId = fiflog_key;
                        rabbitMessage.fileTransferDefinition = fileTransferDefinition;
                        rabbitMessage.serviceId = fileTransferDefinition.firstserviceroute;//Since it is start process
                        rabbitMessage.transferFileName = nameOfFile;
                        rabbitMessage.tmpFolder = fileTransferDefinition.others.tmpfolder;

                        String strRabbitMessage = JsonConvert.SerializeObject(rabbitMessage);
                        var body = Encoding.UTF8.GetBytes(strRabbitMessage);
                        channel.BasicPublish(exchange: GlobalObjects._rabbitFIExchange, routingKey: GlobalObjects._DEFAULTROUTE, basicProperties: null, body: body);
                        //Console.WriteLine(" [x] Sent {0}", strRabbitMessage);

                    }

                }

            }

            return true;

        }


    }




}