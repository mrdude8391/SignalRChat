using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using PVChat.Domain.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRChat
{
    //Summary//
    //The hub is a medium point between server and client that lets the client call methods
    //Clients connect to hubs and then each hub can have different functions 
    //Summar//
    [HubName("PVChatHub")]
    public class ChatHub : Hub
    {
        private static ConcurrentDictionary<string, UserModel> ChatClients = new ConcurrentDictionary<string, UserModel>();
        private static ObservableCollection<MessageModel> MessageDb = new ObservableCollection<MessageModel>();
        public List<UserModel> Login(string name)
        {
            if (!ChatClients.ContainsKey(name))
            {
                Console.WriteLine($"++ {name} logged in");
                List<UserModel> users = new List<UserModel>(ChatClients.Values);
                UserModel newUser = new UserModel
                {
                    ID = Context.ConnectionId,
                    Name = name,
                };
                var added = ChatClients.TryAdd(name, newUser);
                if (!added) return null;
                Clients.CallerState.UserName = name;
                Clients.CallerState.Id = Context.ConnectionId;
                //
                Clients.Others.ParticipantLogin(newUser);
                return users;
            }

            return null;
        }


        public void Logout()
        {
            var name = Clients.CallerState.UserName;
            if (!string.IsNullOrEmpty(name))
            {
                UserModel client = new UserModel();
                ChatClients.TryRemove(name, out client);
                //
                Clients.Others.ParticipantLogout(client);
                Console.WriteLine($"-- { name} logged out");
            }
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var userName = ChatClients.SingleOrDefault(o => o.Value.ID == Context.ConnectionId).Key;
            if (userName != null)
            {
                //
                Clients.Others.ParticipantDisconnection(userName);
                Console.WriteLine($"<> {userName} disconnected");
            }

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            var userName = ChatClients.SingleOrDefault((c) => c.Value.ID == Context.ConnectionId).Key;
            if (userName != null)
            {
                //
                Clients.Others.ParticipantReconnection(userName);
                Console.WriteLine($"== {userName} reconnected");
            }
            return base.OnReconnected();
        }

        public async Task SendMessage(string recepient, string message)
        {
            var sender = Clients.CallerState.UserName;
            if (!string.IsNullOrEmpty(sender) && recepient != sender && !string.IsNullOrEmpty(message) && ChatClients.ContainsKey(recepient))
            {
                UserModel client = new UserModel();
                ChatClients.TryGetValue(recepient, out client);

                MessageModel newMessage = new MessageModel
                {
                    //MessageId = Default
                    SenderId = Clients.CallerState.Id,
                    ReceiverId = client.ID,
                    Message = message,
                    Status = MessageStatus.Sent,
                    //CreatedTime = default
                    SentTime = DateTime.Now,
                };

                MessageDb.Add(newMessage);
                
                //
                await Clients.Client(client.ID).MessageReceived(sender, newMessage);
                await Clients.Caller.MessageSent("Message was sent");
            }
        }
        public async Task MessageDelivered(string sender, MessageModel model)
        {
            if (!string.IsNullOrEmpty(sender))
            {
                UserModel client = new UserModel();
                ChatClients.TryGetValue(sender, out client);

                foreach (var Message in MessageDb
                    .Where(o => o.MessageId == model.MessageId))
                {
                    Message.DeliveredTime = DateTime.Now;
                    Message.Status = MessageStatus.Delivered;
                }
                //
                await Clients.Client(client.ID).MessageDelivered("Message delivered");
            }
        }


        public async Task BroadcastMessage(string message)
        {
            
            // 
            await Clients.All.BroadcastReceived(message);
            Console.WriteLine("Received Message");
        }
    }
}