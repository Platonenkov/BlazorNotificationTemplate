### тестирование примера
* установить в качестве запускаемых проектов Api и Server

### Создать свой сервис
#### 1 Создать приложение Blazor
    1.1 Blazor WebAssembly App
    1.2 ASP.NET Core hosted
    1.3 Progressive Web Application
 
#### 2 Добавить проект Веб-приложение ASP.NET Core
 * Тип шаблона – Api
         
#### 3 В Api установить пакет Microsoft.AspNetCore.SignalR

#### 4 В Api добавить класс
```C#
public class NotificationHub:Hub
    {
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }
    }
```
#### 5 правим файл Startup.cs проекта Api
    5.1 Дописать в ConfigureServices
```C#
services.AddCors(
                o =>
                {
                    o.AddPolicy("CorsPolicy", policy =>
                    {
                        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                    });
                });
services.AddSignalR();
```
    5.2 Дописать в Configure
```C#
app.UseCors("CorsPolicy");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<NotificationHub>("/notificationhub");
                endpoints.MapControllers();
            });
```













#### 6 В проект Shared добавить класс и enum
```C#
public class NotifiMessage
    {
        public string Title { get; set; }
        public DateTime? Time { get; set; } = DateTime.Now;
        public NotifiType Type { get; set; } = NotifiType.none;
        public bool IsPrivate { get; set; } = true;
        public string FromUserId { get; set; }
        public string ToUserId { get; set; }
    }

    public enum NotifiType
    {
        none,
        Debug,
        Info,
        Warning,
        Error
    }
```
#### 7 В проект с Api добавить пустой контроллер Api
 
Листинг
```C# 
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
```
#### 8 В Client установить пакет - ### Microsoft.AspNetCore.SignalR.Client
не путать с ~~Microsoft.AspNet.SignalR.Client~~

#### 9 В свойствах проекта Api взять адрес подключения - у меня это https://localhost:44303/
 
#### 10 В клиент добавить листинг в файл index.razor
```C#
@page "/"
@using Microsoft.Extensions.Logging
@using BlazorNotificationTemplate.Shared
@using Microsoft.AspNetCore.SignalR.Client
@inject HttpClient client
@inject ILogger<Index> Logger

<h3>Connection Status: @connectionStatus</h3>
<div class="row">
    <div class="col-8">
        @foreach (var item in notifications)
        {
            <div class="row card-header">
                <span><b>@item.Type</b><p>@item.Time : @item.Title</p></span>
            </div>
        }
    </div>
</div>


@code{

    #region Notification

    string url = "https://localhost:44303/notificationhub";

    List<NotifiMessage> notifications = new List<NotifiMessage>();
    HubConnection _Connection = null;
    private bool _IsConnected = false;
    bool IsConnected { get; set; }

    string connectionStatus = "Closed";

    protected override async Task OnInitializedAsync()
    {

        await ConnectToSrver();

    }
    private async Task ConnectToSrver()
    {
        _Connection = new HubConnectionBuilder()
            .WithUrl(url)
            .Build();

        await _Connection.StartAsync();
        IsConnected = true;

        connectionStatus = "Connected";

        _Connection.Closed += async (s) =>
        {
            IsConnected = false;
            connectionStatus = "Disconnected";
            await _Connection.StartAsync();
            IsConnected = true;
            connectionStatus = "Connected";
        };

        _Connection.On<NotifiMessage>("notification", m =>
        {
            notifications.Add(m);
            StateHasChanged();
        });
    }

    #endregion

}
```
#### 11 Правой кнопкой на решении – назначить запускаемые проекты
:white_check_mark: Устанавливаем запуск api И сервера приложения
 
#### 12 Запускаем проект для теста соединения
:white_check_mark: Видим что статус подключения через пару секунд меняется на “Connected”
 
#### 13 Создать сервис для уведомлений
    13.1 проект – библиотека Net.Standard 2.1
:white_check_mark: Если создастся 2.0 – проверить в свойствах проекта

    13.2 в проект добавить интерфейс
```C#
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
```
    13.3 установить в проект пакет - System.Net.Http.Json
    13.4 добавить в проект реализацию интерфейса
```C#
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
```
#### Внимательно проверьте https адресс, хост должен быть тот-же что в Inex.razor клиента

#### 14	В проект Server добавить ссылку на проект Сервисов
* подключить реализацию в классе Startup.cs
```C#
            services.AddSingleton<INotificationService, NotificationService>();
```
#### 15	В проекте Server добавить контроллер
```C#
    [ApiController]
    [Route("[controller]")]
    public class NotificationTestController : ControllerBase
    {

        private readonly ILogger<NotificationTestController> logger;
        private readonly INotificationService _Notification;

        public NotificationTestController(ILogger<NotificationTestController> logger, INotificationService notification)
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
```
#### 16	Отредактировать Index.razor проекта Client

    16.1 Добавить разметку
```C#
<button class="btn btn-info" @onclick="StartTest">Start Test</button>
<div class="row">
    <div class="col-8">
        @foreach (var item in notifications)
        {
            <div class="row">
                <h4>@item.Type</h4>
                <h2>@item.Time</h2>
                <h3>@item.Title</h3>
            </div>
        }
    </div>
</div>
```
    16.2 Добавить код вызова контроллера, передав ему ключ вызывающего пользователя
```C#
     async Task StartTest()
    {
        var result = await client.GetAsync($"NotificationTest/GetSomeData/{_Connection.ConnectionId}");
        if (result.IsSuccessStatusCode)
        {

        }

    }
```
#### 17 Редактируем файл gitIgnore
дописываем в конце файла строки
```
# Don't ignore server launchSettings.json. We need a specific port number for auth to work.
!**/BlazorNotificationTemplate.RealTimeApi/Properties/launchSettings.json
```

:white_check_mark: Запускаем проект, статус меняется на “Connected”, жмём старт и видим процесс обработки идущий на сервере
 
 ## Выносим реализацию вывода в Layout для работы с ним на всех окнах
 
 #### 18 переименуем класс и интерфейс
 ```
 INotificationService -> IServerNotificationService
 NotificationService -> ServerNotificationService
```

#### 19 установим в проект с сервисами пакет Microsoft.AspNetCore.SignalR.Client

#### 20 Создадим новый сервис для отображения сообщений на клиенте и переместим в него логику из страницы Index.razor
Вынося реализацию в сервис добавим возможность отслеживать изменения добавив событие OnChange

    20.1 Листинг ClientNotificationService.cs
```C#
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
```
    20.2 Листинг Index.razor
```C#
@page "/"
@using Microsoft.Extensions.Logging
@using BlazorNotificationTemplate.Shared
@using Microsoft.AspNetCore.SignalR.Client
@using BlazorNotificationTemplate.Service.Implementations
@inject HttpClient client
@inject ILogger<Index> Logger
@inject ClientNotificationService NotifiService

<h3>Connection Status: @NotifiService.connectionStatus</h3>
<button class="btn btn-info" @onclick="StartTest">Start Test</button>
<div class="row">
    <div class="col-8">
        @foreach (var item in NotifiService.notifications)
        {
            <div class="row card-header">
                <span><b>@item.Type</b><p>@item.Time : @item.Title</p></span>
            </div>
        }
    </div>
</div>


@code{

    async Task StartTest()
    {
        var result = await client.GetAsync($"NotificationTest/GetSomeData/{NotifiService.UserId}");
        if (result.IsSuccessStatusCode)
        {

        }

    }

}
```
