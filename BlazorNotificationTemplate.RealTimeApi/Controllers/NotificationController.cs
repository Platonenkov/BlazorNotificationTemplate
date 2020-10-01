using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BlazorNotificationTemplate.RealTimeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _HubContext;
        private readonly ILogger<NotificationsController> _Logger;

        public NotificationsController(IHubContext<NotificationHub> hubContext, ILogger<NotificationsController> Logger)
        {
            _HubContext = hubContext;
            _Logger = Logger;
        }

        [HttpGet]
        public string Get()
        {
            return "It work";
        }

        /// <summary>
        /// Получает информационное сообщение отправленное в теле запроса
        /// </summary>
        /// <returns></returns>
        [HttpPost("SendTitle")]
        public async Task<IActionResult> SendTitle()
        {
            using var reader = new StreamReader(Request.Body);
            var json = await reader.ReadToEndAsync();
            var inbound = JsonConvert.DeserializeObject<string>(json);
            await _HubContext.Clients.All.SendAsync("notification", new NotifiMessage { Title = inbound });
            return Ok("Notification has been sent succesfully!");
        }

        /// <summary>
        /// Получает информационное сообщение отправленное в строке
        /// </summary>
        /// <param name="Title">сообщение</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromQuery] string Title)
        {
            await _HubContext.Clients.All.SendAsync("notification", new NotifiMessage { Title = Title });
            return Ok("Notification has been sent succesfully!");
        }

        /// <summary>
        /// Получает информационное сообщение и рассылает его пользователям
        /// </summary>
        /// <param name="message">сообщение</param>
        /// <returns></returns>
        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage(NotifiMessage message)
        {
            try
            {
                var id = message.FromUserId;
                message.FromUserId = string.Empty;

                if (message.IsPrivate)
                    await _HubContext.Clients.Client(id).SendAsync("notification", message);
                else
                    await _HubContext.Clients.All.SendAsync("notification", message);

                return Ok("Notification has been sent succesfully!");
            }
            catch (Exception e)
            {
                return BadRequest($"Error to send notification: {e.Message}");
            }

        }
    }
}
