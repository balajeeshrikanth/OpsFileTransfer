using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpsFileTransfer.DAL;
using OpsFileTransfer.View;


namespace OpsFileTransfer
{

    [ApiController]
    [Route("[controller]")]
    public class OpsFileTransferController : ControllerBase
    {

        [HttpGet]
        public List<MainLogDisplay> Get()
        {
            FiflogDAL fiflogDAL = new FiflogDAL(GlobalObjects._connectionString);
            return fiflogDAL.GetMainLogDisplays();

        }

    }




}
