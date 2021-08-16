using System;
using OpsFileTransfer.Model;
using OpsFileTransfer.DAL;
using System.Collections.Generic;

namespace OpsFileTransfer.View
{

    public class MainLogDisplay
    {

        public string transferDate { get; set; }
        public int transferId { get; set; }

        public string transferType { get; set; }

        public string startDateTime { get; set; }

        public string endDateTime { get; set; }

        public string status { get; set; }

        public string fileName { get; set; }

        public List<LogActivity> logActivities { get; set; }


    }

    public class LogActivity
    {

        public int activityId { get; set; }

        public string serviceId { get; set; }

        public string activityStartTime { get; set; }

        public string activityEndTime { get; set; }

        public string activityStatus { get; set; }


    }






}
