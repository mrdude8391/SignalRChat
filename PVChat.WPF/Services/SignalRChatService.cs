﻿using Microsoft.AspNet.SignalR.Client;
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

        public event Action<UserModel> LoggedIn;
        public event Action ConnectionClosed;
        public event Action<UserModel> ParticipantLogout;
        public event Action<string> BroadcastReceived;
        public event Action<string, MessageModel> MessageReceived;
        public event Action<string> MessageSent;
        public event Action<string> MessageDelivered;
        

        public SignalRChatService(HubConnection connection)
        {
            _connection = connection;
            //Connects to HUB has to be of hub name
            _proxy = _connection.CreateHubProxy("PVChatHub");
            //
            _proxy.On<UserModel>("ParticipantLogin", (u) => LoggedIn?.Invoke(u));
            _proxy.On<UserModel>("ParticipantLogout", (u) => ParticipantLogout?.Invoke(u));
            _proxy.On<string>("BroadcastReceived", (data) => BroadcastReceived?.Invoke(data));
            _proxy.On<string, MessageModel>("MessageReceived", (sender, msg) => MessageReceived?.Invoke(sender, msg));
            _proxy.On<string>("MessageSent", (msg) => MessageSent?.Invoke(msg));
            _proxy.On<string>("MessageDelivered", (confirm) => MessageDelivered?.Invoke(confirm));

            _connection.Closed += Disconnected;
        }


        public async Task Connect()
        {
            await _connection.Start();
            Console.WriteLine("Connected");
        }
        private void Disconnected()
        {
            ConnectionClosed?.Invoke();
        }

        public async Task<List<UserModel>> Login(string Name)
        {
            return await _proxy.Invoke<List<UserModel>>("Login", new object[] { Name });
        }
        public async Task Logout()
        {
            await _proxy.Invoke("Logout");
        }

        public async Task BroadcastMessage(string message)
        {
            await _proxy.Invoke("BroadcastMessage", message);
        }
        
        public async Task SendMessage(string recipient, string message)
        {
            await _proxy.Invoke("SendMessage", new object[] { recipient, message });
        }

        public async Task ConfirmMessageDelivered(string sender, MessageModel model)
        {
            await _proxy.Invoke("MessageDelivered", new object[] { sender, model });
        }
    }
}
