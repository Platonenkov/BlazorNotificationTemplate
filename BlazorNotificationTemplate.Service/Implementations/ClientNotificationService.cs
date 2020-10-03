using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BlazorNotificationTemplate.Shared;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorNotificationTemplate.Service.Implementations
{
    public class ClientNotificationService
    {
        // Lets components receive change notifications
        // Could have whatever granularity you want (more events, hierarchy...)
        public event Action OnChange;
        private void NotifyStateChanged() => OnChange?.Invoke();

        public ClientNotificationService()
        {
            ConnectToServerAsync();
        }
        #region Notification
        public Dictionary<DateTime?, string> events = new Dictionary<DateTime?, string>();

        string url = "https://localhost:44303/notificationhub";

        public ObservableCollection<NotifiMessage> notifications = new ObservableCollection<NotifiMessage>();
        HubConnection _Connection = null;

        public string UserId => _Connection.ConnectionId;

        public string StatusColor { get; set; } = "background-color: red; width: 20px; height: 20px; border-radius: 50%";
        private bool _IsConnected;
        public bool IsConnected
        {
            get => _IsConnected;
            set
            {
                _IsConnected = value;
                StatusColor = value ? "background-color: green; width: 20px; height: 20px; border-radius: 50%" : "background-color: red; width: 20px; height: 20px; border-radius: 50%";
                NotifyStateChanged();
            }
        }
        public string connectionStatus { get; set; } = "Closed";


        void ShowNotification(NotifiMessage message)
        {
            events.Add(message.Time, $"{message.Type}: {message.Title}");
            NotifyStateChanged();
        }
        private async void ConnectToServerAsync()
        {
            notifications.CollectionChanged += async (sender, e) =>
            {
                if (!(sender is ObservableCollection<NotifiMessage>))
                {
                    return;
                }

                foreach (NotifiMessage message in e.NewItems)
                {
                    ShowNotification(message);
                }
            };
            _Connection = new HubConnectionBuilder()
                .WithUrl(url)
                .Build();

            await _Connection.StartAsync();
            IsConnected = true;

            connectionStatus = "Connected";
            NotifyStateChanged();

            _Connection.Closed += async (s) =>
            {
                IsConnected = false;
                connectionStatus = "Disconnected";
                await _Connection.StartAsync();
                IsConnected = true;
                connectionStatus = "Connected";
                NotifyStateChanged();
            };

            _Connection.On<NotifiMessage>("notification", m =>
            {
                notifications.Add(m);
                NotifyStateChanged();
            });
        }

        #endregion
    }
}
