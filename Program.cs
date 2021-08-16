using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Security.Permissions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using OpsFileTransfer.Model;
using System.IO;



namespace OpsFileTransfer
{

    public class Program
    {

        public static IConfigurationRoot configuration;


        public static void Main(string[] args)
        {

            configuration = new ConfigurationBuilder().SetBasePath("/home/balajee/DotNetCoreProjects/OpsFileTransfer/").AddJsonFile("appsettings.json", false).Build();
            GlobalObjects._connectionString = configuration.GetConnectionString("Default");
            //Get Rabbit Details
            GlobalObjects._rabbitHost = configuration.GetSection("RabbitDefinition").GetSection("Host").Value;
            GlobalObjects._rabbitFIExchange = configuration.GetSection("RabbitDefinition").GetSection("FIExchange").Value;
            GlobalObjects._rabbitActivityQueue = configuration.GetSection("RabbitDefinition").GetSection("FIActivityQueue").Value;
            GlobalObjects._rabbitEndQueue = configuration.GetSection("RabbitDefinition").GetSection("FIENDQueue").Value;


            //Get the Transfer definition File
            GlobalObjects._tdfilename = configuration.GetSection("TransferDefinition").GetSection("TFDFileName").Value;
            Console.WriteLine(GlobalObjects._tdfilename);
            //Get Connection String for Database
            //Next Read Transfer Definiton JSON File
            using (StreamReader file = File.OpenText(GlobalObjects._tdfilename))
            {
                Console.WriteLine("Opened File For Reading");
                JsonSerializer serializer = new JsonSerializer();
                GlobalObjects._oFTControl = (OpsFTControl)serializer.Deserialize(file, typeof(OpsFTControl));

            }

            CreateHostBuilder(args).Build().Run();

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).ConfigureServices(services => services.AddHostedService<BackgroundScheduler>())
                .ConfigureServices(services => services.AddHostedService<OpsFTQueueListener>());

    }

    public class GlobalObjects
    {
        public static string _connectionString;
        public static string _rabbitHost;
        public static string _rabbitFIExchange;
        public static string _rabbitActivityQueue;

        public static string _rabbitEndQueue;


        public static OpsFTControl _oFTControl;

        public static string _tdfilename;

        public static string _INPROCESS = "INPROCESS";

        public static string _DEFAULTROUTE = "ALL";

        public static string _END_ROUTE = "END";


    }

}
