using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorNotificationTemplate.Shared;
using BlazorNotificationTemplate.Service;
using BlazorNotificationTemplate.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BlazorNotificationTemplate.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotificationTestController : ControllerBase
    {

        private readonly ILogger<NotificationTestController> logger;
        private readonly IServerNotificationService _Notification;

        public NotificationTestController(ILogger<NotificationTestController> logger, IServerNotificationService notification)
        {
            this.logger = logger;
            _Notification = notification;
        }

        [HttpGet("GetSomeData/{UserId}")]
        public async Task<IActionResult> GetSomeData(string UserId)
        {
            for (var i = 0; i < 10; i++)
            {
                await _Notification.SendNotificationAsync(new NotifiMessage {Title = $"Step {i}", FromUserId = UserId, Type = NotifiType.Info});
                //long operation
                await Task.Delay(1000);
            }
            
            return Ok();
        }
    }

}
