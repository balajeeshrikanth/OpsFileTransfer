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

namespace OpsFileTransfer
{

    public class OpsITEndService
    {
        public FileTransferDefinition filetransferdefinition { get; set; }

        public OpsITEndService()
        {



        }

        public bool EndService(int transferId, FileTransferDefinition fileTransferDefinition, string routingCode, string transferFileName, string tmpFolder)
        {

            //Transfer File to Destination
            if (Directory.Exists(fileTransferDefinition.destination.folder))
            {
                FileInfo fileInfo = new FileInfo(tmpFolder + "/" + transferFileName);
                try
                {
                    fileInfo.MoveTo($@"{fileTransferDefinition.destination.folder}/{transferFileName}");
                    //fileInfo.MoveTo(fileTransferDefinition.destination.folder);
                    string destinationFileName = fileTransferDefinition.destination.folder + "/" + transferFileName;
                    FileInfo fileInfo1 = new FileInfo(destinationFileName);
                    if (!fileInfo1.Exists)
                    {
                        Console.WriteLine("Keep Waiting till transfer complete");

                    }

                }
                catch
                {
                    Console.WriteLine("Unable to move file to destination " + transferFileName);
                }

            }
            else
            {
                Console.WriteLine("Destination not found");
                return false;
            }

            //Mark Main Log entry as completed
            var fiflogdal = new FiflogDAL(GlobalObjects._connectionString);
            if (!fiflogdal.EndFiflog(transferId, "SUCCESS"))
            {
                Console.WriteLine("Unable to mark completion of Transfer");
                return false;
            }

            return true;

        }


    }

}