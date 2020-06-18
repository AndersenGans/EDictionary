using System.Collections.Generic;
using E2Dictionary.BLL;
using E2Dictionary.BLL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace E2Dictionary.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public Test GetTest()
		{
			return MainWorker.GetTest();
		}
    }
}
