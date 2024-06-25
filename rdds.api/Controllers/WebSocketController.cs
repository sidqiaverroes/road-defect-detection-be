using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using rdds.api.Services.MQTT;

namespace rdds.api.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("{deviceId}")]
    public class WebSocketController : ControllerBase
    {
        private readonly MqttService _mqttService;
        private readonly ILogger<WebSocketController> _logger;

        public WebSocketController(MqttService mqttService, ILogger<WebSocketController> logger)
        {
            _mqttService = mqttService;
            _logger = logger;
        }

        [HttpGet]
        public async Task Get([FromRoute] string deviceId)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await _mqttService.HandleWebSocketAsync(webSocket, deviceId);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
    }
}
