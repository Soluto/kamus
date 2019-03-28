using Microsoft.AspNetCore.Mvc;

namespace CustomResourceDescriptorController.Controllers
{
    public class MonitoringController
    {
        [HttpGet]
        [Route("api/v1/isAlive")]
        public bool IsAlive()
        {
            return false;
        }

        [HttpGet]
        [Route("")]
        public string Welcome()
        {
            return "welcome";
        }
    }
}