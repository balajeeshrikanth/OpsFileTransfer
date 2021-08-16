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

namespace OpsFileTransfer
{

    public class TwoFourToGTM
    {

        /*How Does this service Work
        
        Pre-condition - This being invoked by the Rabbit Consumer everytime it reads a message from Rabbit Queue
        1. Identify Current Service That is being executed based on Routing Code From Transfer Definition
        2. Then Get the relevant values for Service ID, Status on completion of current activity and next Service Route
        3. Start current service by placing a record in the activity log with INPROCESS status and FK reference to main log table
        4. Perform the expected service task
        5. On success mark the activity log for current service with the completion status.
        6. Determine next Route
        7. Publish back to Exchange for next stage
    
        
        */

        public TwoFourToGTM()
        {

        }

        public void processService(int transferId, FileTransferDefinition fileTransferDefinition, string routingCode, string transferFileName, string tmpFolder)
        {

            // Get current Service definitions by parsing the fieltransfer Definition for the routing code


            bool serviceFound = false;
            string dbServiceId = "";
            string dbCompletionStatus = "";
            string nextServiceRoute = "";

            for (int i = 0; i < fileTransferDefinition.services.Count; i++)
            {

                OpsService currentService = fileTransferDefinition.services[i];

                if (currentService.routingcode.Equals(routingCode))
                {
                    serviceFound = true;
                    dbServiceId = currentService.serviceid;
                    dbCompletionStatus = currentService.status;
                    nextServiceRoute = currentService.nextserviceroute;
                    break;
                }
            }
            if (serviceFound)
            {
                //First Start with in process in the activity Log entry
                var fifactlogdal = new FifactlogDAL(GlobalObjects._connectionString);
                int fifactlog_key = 0;
                fifactlog_key = fifactlogdal.StartFifactLog(transferId, dbServiceId, GlobalObjects._INPROCESS);
                if (fifactlog_key <= 0)
                {
                    Console.WriteLine("Unable to Insert Start Activity");
                    throw new Exception("error in inserting to database the start step");
                }

                // Here is Where the Main Process for this service Happens
                if (!PerformMainService(tmpFolder, transferFileName))
                {
                    Console.WriteLine("Unable to perform Main task of 24-GTM File Conversion");
                    throw new Exception("Unable to perform Main task of 24-GTM File Conversion");
                }

                //Finally Complete the Activity Record
                if (!fifactlogdal.CompleteFifactLog(fifactlog_key, dbCompletionStatus))
                {
                    Console.WriteLine("Unable to Insert End Activity");
                    throw new Exception("error in inserting to database the start step");
                }

                //Then Publish to Exchange for Next Activity to Take Over
                //This keeps happening till the last Route is END - which will then end processing

                var factory = new ConnectionFactory() { HostName = GlobalObjects._rabbitHost };
                //Publish to Message Queue for pickup
                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.ExchangeDeclare(exchange: GlobalObjects._rabbitFIExchange, type: ExchangeType.Direct, durable: true);
                        RabbitMessage rabbitMessage = new RabbitMessage();
                        rabbitMessage.transferId = transferId;
                        rabbitMessage.fileTransferDefinition = fileTransferDefinition;
                        rabbitMessage.serviceId = GlobalObjects._END_ROUTE;//Next Service
                        rabbitMessage.transferFileName = transferFileName;
                        rabbitMessage.tmpFolder = fileTransferDefinition.others.tmpfolder;

                        String strRabbitMessage = JsonConvert.SerializeObject(rabbitMessage);
                        var body = Encoding.UTF8.GetBytes(strRabbitMessage);
                        channel.BasicPublish(exchange: GlobalObjects._rabbitFIExchange, routingKey: GlobalObjects._DEFAULTROUTE, basicProperties: null, body: body);
                        //Console.WriteLine(" [x] Sent {0}", strRabbitMessage);

                    }

                }

            }
            else
            {
                Console.WriteLine("No relevant service found");
            }

        }

        private bool PerformMainService(String tmpfolder, String filename)
        {

            //For 24-GTM It is about appending 11 Columns at end
            // Then search and get the SGBatchID from FXECUTE DB

            Console.WriteLine("NOW IN THE MAIN 24-GTM SERVICE");
            try
            {
                string sourceFullName = tmpfolder + "/" + filename;
                string outputFileName = sourceFullName + ".tmp";

                using (StreamReader streamReader = new StreamReader(sourceFullName))
                {
                    string line;
                    string appendedLine;
                    string appendData = "\"NA1\",\"NA2\",\"NA3\",\"NA4\",\"NA5\",\"NA6\",\"NA7\",\"NA8\",\"NA9\",\"NA10\",\"NA11\"";

                    List<string> outputLineList = new List<string>();

                    //Read to List
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        //Console.WriteLine(line);
                        appendedLine = line + appendData;
                        outputLineList.Add(appendedLine);
                    }
                    //Write to Tmp Out
                    if (outputLineList.Count > 0)
                    {
                        using (StreamWriter streamWriter = new StreamWriter(outputFileName))
                        {
                            foreach (string outputline in outputLineList)
                            {
                                streamWriter.WriteLine(outputline);
                            }
                        }

                    }
                    else
                    {
                        Console.WriteLine("Nothing to write in output as source is empty");
                    }
                    //Now Delete original file and move the tmp as original
                    try
                    {
                        File.Delete(sourceFullName);
                        File.Move(outputFileName, sourceFullName);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error in File Operation of renaming new to old");
                        return false;
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("source file could not be read " + filename);
                return false;
            }
            return true;
        }

    }

}

