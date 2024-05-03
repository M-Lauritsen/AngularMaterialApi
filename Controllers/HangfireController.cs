using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace AngularMaterialApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HangfireTestController(IBackgroundJobClient jobClient) : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            jobClient.Enqueue(() => Console.WriteLine("Job Client"));
            return Ok("Hello Hangfire Endpoint");
        }
    }
}
