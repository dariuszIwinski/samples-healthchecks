
using Microsoft.AspNetCore.Mvc;

namespace ApiHealthChecks.Controllers
{
    [ApiController]
    [Route("/api/test")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> logger;

        public TestController(ILogger<TestController> logger)
        {
            this.logger = logger;
        }

        [HttpGet("get")]
        public IActionResult Get()
        {
            return Ok();
        }

        [HttpPost("post")]
        public IActionResult Post()
        {
            return Ok();
        }
    }
}