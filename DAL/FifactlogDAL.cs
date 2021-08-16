using OpsFileTransfer.Model;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System;

namespace OpsFileTransfer.DAL
{


    public class FifactlogDAL
    {

        public string _connectionString { get; set; }

        public FifactlogDAL(String connstring)
        {

            this._connectionString = connstring;

        }

        public List<FifactlogModel> GetListForTransferId(int transferId)
        {

            var fifactlogModelList = new List<FifactlogModel>();

            try
            {

                string oString = "SELECT * from FIFACTLOG WHERE transferid=@transferId";
                using (SqlConnection appConnection = new SqlConnection(_connectionString))
                {

                    SqlCommand oCmd = new SqlCommand(oString, appConnection);
                    oCmd.Parameters.AddWithValue("@transferId", transferId);
                    appConnection.Open();
                    using (SqlDataReader oReader = oCmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            FifactlogModel fifactlogModel = new FifactlogModel();
                            fifactlogModel.activityid = Convert.ToInt32(oReader["activityid"]);
                            fifactlogModel.transferid = Convert.ToInt32(oReader["transferid"]);
                            fifactlogModel.serviceid = oReader["serviceid"].ToString();
                            fifactlogModel.activitystarttime = oReader["activitystarttime"].ToString();
                            fifactlogModel.activityendtime = oReader["activityendtime"].ToString();
                            fifactlogModel.status = oReader["status"].ToString();
                            fifactlogModelList.Add(fifactlogModel);
                        }

                    }

                }

            }
            catch
            {
                throw;
            }
            return fifactlogModelList;
        }

        public int StartFifactLog(int transferId, string serviceId, string status)
        {
            int result = 0;
            try
            {
            
                //For Exception Handling Reasons -

                
                DateTime dt = DateTime.Now;
                string oString = "INSERT INTO dbo.FIFACTLOG (transferid, serviceid, activitystarttime, status) VALUES (@transferid, @serviceid, @activitystarttime, @status);SELECT SCOPE_IDENTITY(); ";
                using (SqlConnection appConnection = new SqlConnection(_connectionString))
                {

                    SqlCommand oCmd = new SqlCommand(oString, appConnection);
                    oCmd.Parameters.AddWithValue("@transferid", transferId);
                    oCmd.Parameters.AddWithValue("@serviceid", serviceId);
                    oCmd.Parameters.AddWithValue("@activitystarttime", dt);
                    oCmd.Parameters.AddWithValue("@status", status);
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

        public bool CompleteFifactLog(int activityId, string status)
        {
            try
            {

                DateTime dt = DateTime.Now;
                string oString = "UPDATE dbo.FIFACTLOG SET activityendtime = @activityendtime, status = @status WHERE activityid = @activityid";
                using (SqlConnection appConnection = new SqlConnection(_connectionString))
                {
                    SqlCommand oCmd = new SqlCommand(oString, appConnection);
                    oCmd.Parameters.AddWithValue("@activityendtime", dt);
                    oCmd.Parameters.AddWithValue("@status", status);
                    oCmd.Parameters.AddWithValue("@activityid", activityId);

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