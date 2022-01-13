using Microsoft.AspNet.SignalR.Client;
using PVChat.Domain.Models;
using PVChat.Domain.Services;
using PVChat.WPF.Services;
using SignalRChat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVChat.WPF.Services
{
    public class SignalRChatService : ISignalRChatService
    {
        private readonly HubConnection _connection;
        private readonly IHubProxy _proxy;

        public event Action<ParticipantModel> LoggedIn;
        public event Action ConnectionClosed;
        public event Action<ParticipantModel> ParticipantLogout;
        public event Action<MessageModel> MessageReceived;
        public event Action<MessageModel> MessageSent;
        public event Action<MessageModel> MessageDelivered;
        

        public SignalRChatService(HubConnection connection)
        {
            _connection = connection;
            //Connects to HUB has to be of hub name
            _proxy = _connection.CreateHubProxy("PVChatHub");
            //
            _proxy.On<ParticipantModel>("ParticipantLogin", (u) => LoggedIn?.Invoke(u));
            _proxy.On<ParticipantModel>("ParticipantLogout", (u) => ParticipantLogout?.Invoke(u));
            _proxy.On<MessageModel>("MessageReceived", (msg) => MessageReceived?.Invoke(msg));
            _proxy.On<MessageModel>("MessageSent", (msg) => MessageSent?.Invoke(msg));
            _proxy.On<MessageModel>("MessageDelivered", (msg) => MessageDelivered?.Invoke(msg));

            _connection.Closed += Disconnect;
        }


        public async Task Connect()
        {
            await _connection.Start();
            Console.WriteLine("Connected");
        }
        private void Disconnect()
        {
            ConnectionClosed?.Invoke();
        }

        public async Task<List<ParticipantModel>> Login(string Name, string Database)
        {
            return await _proxy.Invoke<List<ParticipantModel>>("Login", new object[] { Name , Database });
        }
        public async Task Logout()
        {
            await _proxy.Invoke("Logout");
        }

        public async Task<List<MessageModel>> GetMessages(ParticipantModel user)
        {
            return await _proxy.Invoke<List<MessageModel>>("GetMessages", user);
        }
        
        public async Task SendMessage(ParticipantModel recepient, MessageModel message)
        {
            await _proxy.Invoke("SendMessage", new object[] { recepient, message });
        }

        public async Task ConfirmMessageDelivered(ParticipantModel sender, MessageModel message)
        {
            await _proxy.Invoke("MessageDelivered", new object[] { sender, message });
        }
    }
}
