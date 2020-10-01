using System.Threading.Tasks;
using BlazorNotificationTemplate.Shared;

namespace BlazorNotificationTemplate.Service
{
    public interface INotificationService
    {
        public Task<bool> SendNotificationAsync(NotifiMessage message);
        public Task<bool> SendNotificationAsync(string Title);

    }
}
