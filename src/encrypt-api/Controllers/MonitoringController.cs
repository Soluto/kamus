using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Kamus.Controllers
{
    public class MonitoringController
    {
        [HttpGet]
        [Route("api/v1/isAlive")]
        public bool IsAlive()
        {
            return true;
        }

        [HttpGet]
        [Route("")]
        public string Welcome()
        {
            return "welcome";
        }
    }
}