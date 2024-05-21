using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using rdds.api.Services.MQTT;

namespace rdds.api.Controllers
{
    public class WebSocketController : ControllerBase
    {
        private readonly MqttService _mqttService;

        public WebSocketController(MqttService mqttService)
        {
            _mqttService = mqttService;
        }
        
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("/{deviceId}/{attemptId}")]
        public async Task Get([FromRoute] string deviceId, [FromRoute] string attemptId)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await _mqttService.HandleWebSocketAsync(webSocket, deviceId, attemptId);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        
    }
}