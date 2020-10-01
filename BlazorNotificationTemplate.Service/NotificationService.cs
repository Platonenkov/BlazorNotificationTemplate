using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorNotificationTemplate.Shared;
using Microsoft.Extensions.Logging;

namespace BlazorNotificationTemplate.Service
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _Logger;

        public NotificationService(ILogger<NotificationService> Logger)
        {
            _Logger = Logger;
        }

        #region Implementation of INotificationService

        public async Task<bool> SendNotificationAsync(NotifiMessage message)
        {
            try
            {
                using var response = await new HttpClient().PostAsJsonAsync("https://localhost:44303/api/notifications/SendMessage", message);
                return response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                _Logger.LogError("Ошибка отправки сообщения на сервер: {error}", e.Message);
                return false;
            }
        }
        public async Task<bool> SendNotificationAsync(string Title)
        {
            try
            {
                using var response = await new HttpClient().PostAsJsonAsync("https://localhost:44303/api/notifications/SendTitle", Title);
                return response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                _Logger.LogError("Ошибка отправки сообщения на сервер: {error}", e.Message);
                return false;
            }
        }
        #endregion
    }
}
