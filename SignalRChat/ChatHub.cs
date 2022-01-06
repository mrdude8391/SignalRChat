using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using PVChat.Domain.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
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
                    Id = Context.ConnectionId,
                    Name = name,
                };
                var added = ChatClients.TryAdd(name, newUser);
                if (!added) return null;

                Groups.Add(Context.ConnectionId, name);

                Clients.CallerState.UserName = name;
                Clients.CallerState.Id = Context.ConnectionId;
                //
                Clients.Others.ParticipantLogin(newUser);
                return users;
            }
            else if (ChatClients.ContainsKey(name))
            {
                UserModel client = new UserModel();
                ChatClients.TryRemove(name, out client);

                List<UserModel> users = new List<UserModel>(ChatClients.Values);
                UserModel newUser = new UserModel
                {
                    Id = Context.ConnectionId,
                    Name = name,
                };
                var added = ChatClients.TryAdd(name, newUser);
                if (!added) return null;

                Groups.Add(Context.ConnectionId, name);

                Clients.CallerState.UserName = name;
                Clients.CallerState.Id = Context.ConnectionId;
                //
                //Clients.Others.ParticipantLogin(newUser);
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
                //ChatClients.TryRemove(name, out client);
                //
                Clients.Others.ParticipantLogout(client);
                Console.WriteLine($"-- { name} logged out");
            }
        }



        public override Task OnDisconnected(bool stopCalled)
        {
            var userName = ChatClients.SingleOrDefault(o => o.Value.Id == Context.ConnectionId).Key;
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
            var userName = ChatClients.SingleOrDefault((c) => c.Value.Id == Context.ConnectionId).Key;
            if (userName != null)
            {
                //
                Clients.Others.ParticipantReconnection(userName);
                Console.WriteLine($"== {userName} reconnected");
            }
            return base.OnReconnected();
        }

        public List<MessageModel> GetMessages(UserModel user)
        {
            var sender = Clients.CallerState.UserName;
            var target = user.Name;
            ObservableCollection<MessageModel> messages = new ObservableCollection<MessageModel>();
            foreach (var msg in MessageDb
                .Where(o => o.SenderName == sender || o.SenderName == target)
                .Where(o => o.ReceiverName == target || o.ReceiverName == sender))
            {
                if(sender == msg.SenderName)
                {
                    msg.IsOriginNative = true;
                }
                else
                {
                    msg.IsOriginNative = false;
                }
                messages.Add(msg);
            }

            return messages.ToList();
        }

        public async Task SendMessage(string recepient, MessageModel message)
        {
            
            var sender = Clients.CallerState.UserName;
            if (!string.IsNullOrEmpty(sender) && recepient != sender && !string.IsNullOrEmpty(message.Message) && ChatClients.ContainsKey(recepient))
            {

                UserModel client = new UserModel();
                ChatClients.TryGetValue(recepient, out client);

                message.SenderId = Clients.CallerState.Id;
                message.SenderName = Clients.CallerState.UserName;
                message.Status = MessageStatus.Sent;
                message.SentTime = DateTime.Now;
                message.IsOriginNative = false;

                MessageDb.Add(message);

                //
                
                await Clients.Group(recepient).MessageReceived(message);

                //await Clients.Client(client.Id).MessageReceived(message);
                await Clients.Caller.MessageSent(message);
            }
        }

        public async Task MessageDelivered(MessageModel message)
        {
            var sender = message.SenderName;
            if (!string.IsNullOrEmpty(sender))
            {
                
                foreach (var Message in MessageDb
                    .Where(o => o.MessageId == message.MessageId))
                {
                    Message.DeliveredTime = DateTime.Now;
                    Message.Status = MessageStatus.Delivered;
                }
                //
                await Clients.Group(sender).MessageDelivered("Message delivered");
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