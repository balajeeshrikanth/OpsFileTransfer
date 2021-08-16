using System.Collections.Generic;
using System;
using System.IO;


namespace OpsFileTransfer.Model
{

    public class OpsFTControl
    {
        public List<FileTransferDefinition> oftcontrol { get; set; }

    }

    public class FileTransferDefinition
    {
        public string transfertypeid { get; set; }
        public bool active { get; set; }

        public string firstserviceroute { get; set; }
        public Location source { get; set; }

        public List<OpsService> services;
        public Location destination { get; set; }
        public Others others { get; set; }

        public string getTransferTypeId()
        {
            return this.transfertypeid;
        }

        public string getSourceDirectory()
        {
            return this.source.folder;
        }

        public string getTmpDirectory()
        {
            return this.others.tmpfolder;
        }

    }

    public class Location
    {
        public string folder { get; set; }
        public string filepattern { get; set; }

    }

    public class Others
    {
        public bool emailalert { get; set; }
        public string errorrouting { get; set; }

        public string tmpfolder { get; set; }
    }

    public class OpsService
    {
        public string routingcode { get; set; }
        public string serviceid { get; set; }
        public string status { get; set; }
        public string nextserviceroute { get; set; }

    }


    public class FiflogModel
    {
        public int transferid { get; set; }
        public string transferdate { get; set; }
        public string starttime { get; set; }

        public string endtime { get; set; }

        public string status { get; set; }

        public string filename { get; set; }

        public string tfid { get; set; }


    }

    public class FifactlogModel
    {

        public int activityid { get; set; }
        public int transferid { get; set; }

        public string serviceid { get; set; }

        public string activitystarttime { get; set; }

        public string activityendtime { get; set; }

        public string status { get; set; }




    }

    public class RabbitMessage
    {
        public int transferId { get; set; }
        public FileTransferDefinition fileTransferDefinition { get; set; }

        public string serviceId { get; set; }

        public string transferFileName { get; set; }

        public string tmpFolder { get; set; }



    }




}