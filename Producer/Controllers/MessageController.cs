using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Producer.Models;
using Producer.Services;

namespace Producer.Controllers
{
    [Route("api/producer")]
    [ApiController]
    public class MessageController: ControllerBase
    {
        private readonly ProducerService _producerService;

        public MessageController(ProducerService producerService)
        {
            _producerService = producerService;
        }

        [HttpGet("test")] // Endpoint to test the host
        public IActionResult TestHost()
        {
            return Ok("Hello from the API!");
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            MessageUpdateRequest message = new MessageUpdateRequest{
                Id = 123,
                ProductId = "test",
                Quantity = 3
            };

            return Ok(message);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateInventory([FromBody] MessageUpdateRequest request)
        {
            var message = JsonSerializer.Serialize(request);

            await _producerService.ProduceAsync("test-tp", message);

            return Ok("Inventory Updated Successfully...");
        }
    }
}