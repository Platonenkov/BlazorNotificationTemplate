using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorNotificationTemplate.Service.Interfaces;
using BlazorNotificationTemplate.Shared;
using Microsoft.Extensions.Logging;

namespace BlazorNotificationTemplate.Service.Implementations
{
    public class ServerNotificationService : IServerNotificationService
    {
        private readonly ILogger<ServerNotificationService> _Logger;

        public ServerNotificationService(ILogger<ServerNotificationService> Logger)
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
