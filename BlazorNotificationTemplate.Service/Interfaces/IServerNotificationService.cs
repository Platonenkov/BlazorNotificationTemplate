using System.Threading.Tasks;
using BlazorNotificationTemplate.Shared;

namespace BlazorNotificationTemplate.Service.Interfaces
{
    public interface IServerNotificationService
    {
        public Task<bool> SendNotificationAsync(NotifiMessage message);
        public Task<bool> SendNotificationAsync(string Title);

    }
}
