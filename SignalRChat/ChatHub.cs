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
       
        private static ConcurrentDictionary<string, ParticipantModel> ChatClients = new ConcurrentDictionary<string, ParticipantModel>();
        private static ObservableCollection<MessageModel> MessageDb = new ObservableCollection<MessageModel>();
        

        public async Task<List<ParticipantModel>> Login(string name)
        {
            if (!ChatClients.ContainsKey(name)) // new user connects to hub
            {
                Console.WriteLine($"++ {name} logged in");
                List<ParticipantModel> participants = new List<ParticipantModel>(ChatClients.Values); // get list of participants
                ParticipantModel newParticipant = new ParticipantModel
                {
                    Id = Context.ConnectionId,
                    Name = name,
                    Online = true,
                };
                var added = ChatClients.TryAdd(name, newParticipant); // add user to list of clients connected 
                if (!added) return null;

                await Groups.Add(Context.ConnectionId, name); // add id to group name. Group is same name as username

                Clients.CallerState.UserName = name; // set callerstate username
                Clients.CallerState.Id = Context.ConnectionId; // set callerstate id

                foreach (var participant in participants)
                {
                    participant.Messages = new ObservableCollection<MessageModel>(await GetMessagesOnLogin(participant)); // populate participants with messages 
                }

                Clients.Others.ParticipantLogin(newParticipant); // update list of participants for all other clients
                return participants; // return list of connected participants
            }
            else if (ChatClients.ContainsKey(name)) // returning user same username
            {
                Clients.CallerState.UserName = name;
                Clients.CallerState.Id = Context.ConnectionId;

                ParticipantModel client = new ParticipantModel();
                ChatClients.TryRemove(name, out client);

                await Groups.Add(Context.ConnectionId, name); // add id to group name. Group is same name as username

                List<ParticipantModel> participants = new List<ParticipantModel>(ChatClients.Values); // add back with new connection id
                
                foreach (var participant in participants)
                {
                    participant.Messages = new ObservableCollection<MessageModel>(await GetMessagesOnLogin(participant)); // Populate Participant List with messages
                }

                ParticipantModel newParticipant = new ParticipantModel
                {
                    Id = Context.ConnectionId,
                    Name = name,
                    Online = true,
                };
                var added = ChatClients.TryAdd(name, newParticipant); // add user to list of clients connected 
                if (!added) return null;
                //
                Clients.Others.ParticipantLogin(newParticipant); 
                return participants; // return list of connected users
            }
                        
            return null;
        }

        

        public void Logout()
        {
            var name = Clients.CallerState.UserName;
            if (!string.IsNullOrEmpty(name))
            {
                ParticipantModel client = new ParticipantModel
                {
                    Id = Context.ConnectionId,
                    Name = name,
                    Online = false,
                };
                ChatClients[name] = client;
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

        public List<MessageModel> GetMessages(ParticipantModel participant) // Gets messages for selected user and the caller
        {
            var sender = participant.Name;
            var receiver = Clients.CallerState.UserName;
            ObservableCollection<MessageModel> messages = new ObservableCollection<MessageModel>();
            foreach (var msg in MessageDb
                .Where(o => o.SenderName == sender || o.SenderName == receiver)
                .Where(o => o.ReceiverName == receiver || o.ReceiverName == sender))
            {
                if(sender == msg.SenderName)
                {
                    msg.IsOriginNative = true;
                }
                else
                {
                    msg.IsOriginNative = false;
                }
                msg.Unread = false;
                messages.Add(msg);
                
            }

            return messages.ToList();
        }
        public async Task<List<MessageModel>> GetMessagesOnLogin(ParticipantModel participant) // Gets messages for selected user and the caller
        {
            var sender = Clients.CallerState.UserName;
            var receiver = participant.Name;
            ObservableCollection<MessageModel> messages = new ObservableCollection<MessageModel>();
            foreach (var msg in MessageDb
                .Where(o => o.SenderName == sender || o.SenderName == receiver)
                .Where(o => o.ReceiverName == receiver || o.ReceiverName == sender))
            {
                if (sender == msg.SenderName)
                {
                    msg.IsOriginNative = true;
                }
                else
                {
                    msg.IsOriginNative = false;
                }
                messages.Add(msg);
                await MessageDelivered(msg);
            }

            return messages.ToList();
        }

        public async Task SendMessage(string recepient, MessageModel message)
        {
            
            var sender = Clients.CallerState.UserName;
            if (!string.IsNullOrEmpty(sender) && recepient != sender && !string.IsNullOrEmpty(message.Message) && ChatClients.ContainsKey(recepient))
            {

                message.SenderId = Clients.CallerState.Id;
                message.SenderName = Clients.CallerState.UserName;
                message.Status = MessageStatus.Sent;
                message.SentTime = DateTime.Now;
                message.IsOriginNative = false;

                MessageDb.Add(message);

                //
                
                await Clients.Group(recepient).MessageReceived(message); // Send message to the connection Ids under the groupname

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
                    message = Message;
                }
                
                //
                await Clients.Group(sender).MessageDelivered(message);
            }
        }

        
    }
}