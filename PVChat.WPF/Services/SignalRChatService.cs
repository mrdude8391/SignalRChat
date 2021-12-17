using Microsoft.AspNet.SignalR.Client;
using SignalRChat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVChat.WPF.Services
{
    public class SignalRChatService
    {
        private readonly HubConnection _connection;
        private readonly IHubProxy _proxy;

        public event Action<string> MessageReceived;

        public SignalRChatService(HubConnection connection)
        {
            _connection = connection;
            _proxy = _connection.CreateHubProxy("ChatHub");

            _proxy.On<string>("ReceiveMessage", (data) =>
                MessageReceived?.Invoke(data)
            );

        }

        public async Task Connect()
        {
            await _connection.Start();
        }

        public async Task SendMessage(string message)
        {
            await _proxy.Invoke("NewMessage", message);
        }

    }
}
