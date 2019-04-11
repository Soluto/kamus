using System;
using System.Linq;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace CustomResourceDescriptorController.Controllers
{
    public class MonitoringController : Controller
    {
        [HttpGet]
        [Route("")]
        public string Welcome()
        {
            return "welcome";
        }
    }
}