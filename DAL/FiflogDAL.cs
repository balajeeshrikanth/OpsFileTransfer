using OpsFileTransfer.Model;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System;
using OpsFileTransfer.View;

namespace OpsFileTransfer.DAL
{

    public class FiflogDAL
    {

        public string _connectionString { get; set; }

        public FiflogDAL(string connstring)
        {
            this._connectionString = connstring;
        }

        public List<FiflogModel> GetAll()
        {

            var fiflogModelList = new List<FiflogModel>();

            try
            {
                string oString = "SELECT * from FIFLOG";
                using (SqlConnection appConnection = new SqlConnection(_connectionString))
                {
                    SqlCommand oCmd = new SqlCommand(oString, appConnection);
                    appConnection.Open();
                    using (SqlDataReader oReader = oCmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            FiflogModel fiflogModel = new FiflogModel();
                            fiflogModel.transferid = Convert.ToInt32(oReader["transferid"]);
                            fiflogModel.transferdate = oReader["transferdate"].ToString();
                            fiflogModel.starttime = oReader["starttime"].ToString();
                            fiflogModel.endtime = oReader["endtime"].ToString();
                            fiflogModel.status = oReader["status"].ToString();
                            fiflogModel.filename = oReader["filename"].ToString();
                            fiflogModel.tfid = oReader["tfid"].ToString();
                            fiflogModelList.Add(fiflogModel);
                        }
                    }
                }

            }
            catch
            {
                throw;
            }

            return fiflogModelList;
        }
        public List<FiflogModel> GetListForDate(String queryDate)
        {

            var fiflogModelList = new List<FiflogModel>();

            try
            {
                string oString = "SELECT * from FIFLOG WHERE transferDate=@tDate";
                DateTime transferdt = Convert.ToDateTime(queryDate);
                using (SqlConnection appConnection = new SqlConnection(_connectionString))
                {
                    SqlCommand oCmd = new SqlCommand(oString, appConnection);
                    oCmd.Parameters.AddWithValue("@tDate", transferdt);
                    appConnection.Open();
                    using (SqlDataReader oReader = oCmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            FiflogModel fiflogModel = new FiflogModel();
                            fiflogModel.transferid = Convert.ToInt32(oReader["transferid"]);
                            fiflogModel.transferdate = oReader["transferdate"].ToString();
                            fiflogModel.starttime = oReader["starttime"].ToString();
                            fiflogModel.endtime = oReader["endtime"].ToString();
                            fiflogModel.status = oReader["status"].ToString();
                            fiflogModel.filename = oReader["filename"].ToString();
                            fiflogModel.tfid = oReader["tfid"].ToString();
                            fiflogModelList.Add(fiflogModel);
                        }
                    }
                }

            }
            catch
            {
                throw;
            }

            return fiflogModelList;
        }

        public List<FiflogModel> GetListForFileName(String filename, String transferTypeId)
        {

            var fiflogModelList = new List<FiflogModel>();

            try
            {
                string oString = "SELECT * from FIFLOG WHERE filename=@filename AND tfid=@tfid";
                using (SqlConnection appConnection = new SqlConnection(_connectionString))
                {
                    SqlCommand oCmd = new SqlCommand(oString, appConnection);
                    oCmd.Parameters.AddWithValue("@filename", filename);
                    oCmd.Parameters.AddWithValue("@tfid", transferTypeId);

                    appConnection.Open();
                    using (SqlDataReader oReader = oCmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            FiflogModel fiflogModel = new FiflogModel();
                            fiflogModel.transferid = Convert.ToInt32(oReader["transferid"]);
                            fiflogModel.transferdate = oReader["transferdate"].ToString();
                            fiflogModel.starttime = oReader["starttime"].ToString();
                            fiflogModel.endtime = oReader["endtime"].ToString();
                            fiflogModel.status = oReader["status"].ToString();
                            fiflogModel.filename = oReader["filename"].ToString();
                            fiflogModel.tfid = oReader["tfid"].ToString();
                            fiflogModelList.Add(fiflogModel);
                        }
                    }
                }

            }
            catch
            {
                throw;
            }

            return fiflogModelList;
        }

        public List<MainLogDisplay> GetMainLogDisplays()
        {

            List<MainLogDisplay> mainLogDisplays = new List<MainLogDisplay>();
            List<FiflogModel> fiflogModels = GetAll();
            if (fiflogModels.Count > 0)
            {
                foreach (FiflogModel fiflogModel in fiflogModels)
                {

                    MainLogDisplay mainLogDisplay = new MainLogDisplay();
                    mainLogDisplay.transferId = fiflogModel.transferid;
                    mainLogDisplay.transferDate = fiflogModel.transferdate.ToString();
                    mainLogDisplay.startDateTime = fiflogModel.starttime.ToString();
                    mainLogDisplay.endDateTime = fiflogModel.endtime.ToString();
                    mainLogDisplay.status = fiflogModel.status;
                    mainLogDisplay.transferType = fiflogModel.tfid;

                    FifactlogDAL fifactlogDAL = new FifactlogDAL(_connectionString);
                    List<FifactlogModel> fifactlogModels = fifactlogDAL.GetListForTransferId(fiflogModel.transferid);
                    List<LogActivity> logActivities = new List<LogActivity>();
                    foreach (FifactlogModel fifactlogModel in fifactlogModels)
                    {
                        LogActivity logActivity = new LogActivity();
                        logActivity.activityId = fifactlogModel.activityid;
                        logActivity.activityStartTime = fifactlogModel.activitystarttime.ToString();
                        logActivity.activityEndTime = fifactlogModel.activityendtime.ToString();
                        logActivity.activityStatus = fifactlogModel.status;
                        logActivity.serviceId = fifactlogModel.serviceid;
                        logActivities.Add(logActivity);
                    }
                    mainLogDisplay.logActivities = logActivities;
                    mainLogDisplays.Add(mainLogDisplay);

                }

                return mainLogDisplays;


            }



            return mainLogDisplays;

        }

        public int StartFiflog(string filename, string transferTypeId)
        {

            int result = 0;
            DateTime dt = DateTime.Now;

            //First Check if such a file was attempted earlier and aborted
            try
            {
                List<FiflogModel> fifloglist = GetListForFileName(filename, transferTypeId);
                if (fifloglist.Count > 0)
                {
                    //Earlier Record Exists
                    //Ideally one record only should exist
                    if (fifloglist.Count > 1)
                    {
                        return result;//Exit From routine
                    }

                    // Find if it continues to be in START 
                    FiflogModel fiflogModel = fifloglist[0];
                    if (fiflogModel.status == "START")
                    {
                        string oString = "UPDATE dbo.FIFLOG SET starttime = @starttime WHERE transferid = @transferid";
                        using (SqlConnection appConnection = new SqlConnection(_connectionString))
                        {
                            SqlCommand oCmd = new SqlCommand(oString, appConnection);
                            oCmd.Parameters.AddWithValue("@starttime", dt);
                            oCmd.Parameters.AddWithValue("@transferid", fiflogModel.transferid);

                        }
                        return fiflogModel.transferid;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception while finding existing record in Log " + e.Message);
            }



            try
            {
                string oString = "INSERT INTO dbo.FIFLOG (transferdate, starttime,  status, filename,tfid) VALUES (@transferdate, @starttime,  @status, @filename, @tfid);SELECT SCOPE_IDENTITY();";
                using (SqlConnection appConnection = new SqlConnection(_connectionString))
                {
                    SqlCommand oCmd = new SqlCommand(oString, appConnection);
                    oCmd.Parameters.AddWithValue("@transferdate", dt);
                    oCmd.Parameters.AddWithValue("@starttime", dt);
                    oCmd.Parameters.AddWithValue("@status", "START");
                    oCmd.Parameters.AddWithValue("@filename", filename);
                    oCmd.Parameters.AddWithValue("@tfid", transferTypeId);
                    appConnection.Open();
                    string result1 = oCmd.ExecuteScalar().ToString();
                    result = int.Parse(result1);
                    if (result <= 0)
                    {
                        Console.WriteLine("Error in Insert");
                    }
                }

            }
            catch
            {
                throw;
            }
            return result;
        }

        public bool EndFiflog(int transferId, string errorString)
        {
            try
            {
                DateTime dt = DateTime.Now;
                string oString = "UPDATE  dbo.FIFLOG SET endtime = @endtime, status = @status WHERE transferid = @transferid";
                using (SqlConnection appConnection = new SqlConnection(_connectionString))
                {
                    SqlCommand oCmd = new SqlCommand(oString, appConnection);
                    oCmd.Parameters.AddWithValue("@endtime", dt);
                    oCmd.Parameters.AddWithValue("@status", "END");
                    oCmd.Parameters.AddWithValue("@transferid", transferId);

                    appConnection.Open();
                    int result = oCmd.ExecuteNonQuery();
                    if (result < 0)
                    {
                        Console.WriteLine("Error in Update End Time and Status");
                        return false;
                    }
                }

            }
            catch
            {
                throw;
            }
            return true;
        }

    }



}