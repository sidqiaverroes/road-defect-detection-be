using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consumer.Models;
using Microsoft.AspNetCore.Mvc;

namespace Consumer.Controllers
{
    [Route("api/consumer")]
    [ApiController]
    public class ConsumerController: ControllerBase
    {
        private readonly ILogger<ConsumerController> _logger;

        public ConsumerController(ILogger<ConsumerController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult ProcessInventoryUpdate([FromBody] ConsumerGetRequest request)
        {
            return Ok("Inventory update processed successfully.");
        }
    }
}