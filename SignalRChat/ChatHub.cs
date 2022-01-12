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
        private static ConcurrentDictionary<string, ConcurrentDictionary<string, ParticipantModel>> ChatClientsOfDb = new ConcurrentDictionary<string, ConcurrentDictionary<string, ParticipantModel>>();
        private static ConcurrentDictionary<string, ParticipantModel> ChatClients = new ConcurrentDictionary<string, ParticipantModel>();
        private static ObservableCollection<MessageModel> MessageDb = new ObservableCollection<MessageModel>();

        public ChatHub()
        {
            var db = "NS_001";
            ChatClientsOfDb.TryAdd(db, new ConcurrentDictionary<string, ParticipantModel>());
        }

        public async Task<List<ParticipantModel>> Login(string name, string database)
        {
            if (!ChatClientsOfDb[database].ContainsKey(name)) // if new user in new database
            {
                Console.WriteLine($"++ {name} logged in");
                Clients.CallerState.UserName = name; // set callerstate username
                Clients.CallerState.Id = Context.ConnectionId; // set callerstate id
                await Groups.Add(Context.ConnectionId, name); // add id to group name. Group is same name as username

                List<ParticipantModel> participants = ChatClientsOfDb[database].Values.ToList(); // get list of participants to give to user logging in
                List<string> participantConnections = GetAllConnectionIds(participants); //get connection ids of participants

                ParticipantModel newParticipant = new ParticipantModel
                {
                    Id = Context.ConnectionId,
                    Name = name,
                    Online = true,
                    DatabaseName = database,
                };
                newParticipant.Connections.Add(Context.ConnectionId);


                var addToChatClients = ChatClientsOfDb[database].TryAdd(name, newParticipant); // add user to list of all clients connected
                if (!addToChatClients) return null;

                foreach (var participant in participants) // get all messages from participants to user logging on
                {
                    participant.Messages = new ObservableCollection<MessageModel>(await GetMessagesOnLogin(participant));
                }

                //Clients.Others.ParticipantLogin(newParticipant); // Tell other clients that a new user logged in

                Clients.Clients(participantConnections).ParticipantLogin(newParticipant); // Tell all clients in the database that a new user logged in

                return participants; // return list of connected participants
            }
            else if (ChatClientsOfDb[database].ContainsKey(name)) // returning user same username and user is offline && ChatClientsOfDb[database][name].Online == false
            {
                Clients.CallerState.UserName = name;
                Clients.CallerState.Id = Context.ConnectionId;
                await Groups.Add(Context.ConnectionId, name); // add id to group name. Group is same name as username
                ParticipantModel client = new ParticipantModel();
                ChatClients.TryRemove(name, out client);

                List<ParticipantModel> participants = ChatClientsOfDb[database].Values.ToList(); // get all participants including user logging in
                participants.Remove(participants.Where(p => p.Name == name).FirstOrDefault()); // remove user logging in from list of participants

                List<string> participantConnections = GetAllConnectionIds(participants); // get all connections of participants 


                foreach (var participant in participants) // Populate Participant List with messages
                {
                    participant.Messages = new ObservableCollection<MessageModel>(await GetMessagesOnLogin(participant));
                }

                //Change status of user to online and add connection id 
                ChatClientsOfDb[database][name].Online = true;
                if (!ChatClientsOfDb[database][name].Connections.Contains(Context.ConnectionId))
                    ChatClientsOfDb[database][name].Connections.Add(Context.ConnectionId);

                //

                //Clients.Others.ParticipantLogin(newParticipant);
                Clients.Clients(participantConnections).ParticipantLogin(ChatClientsOfDb[database][name]); // ping other participants of user login

                return participants; // return list of connected users
            }

            return null;
        }


        public List<string> GetAllConnectionIds(List<ParticipantModel> participants)
        {
            List<string> connections = new List<string>();

            foreach (var p in participants)
            {
                connections.AddRange(p.Connections);
            }

            return connections;
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
                if (receiver == msg.ReceiverName) //if the receiver of the message is the user
                {
                    msg.IsOriginNative = false;
                }
                else
                {
                    msg.IsOriginNative = true;
                }
                msg.Unread = false;
                messages.Add(msg);
            }

            return messages.ToList();
        }

        public async Task<List<MessageModel>> GetMessagesOnLogin(ParticipantModel participant) // Gets messages for selected user and the caller
        {
            var sender = participant.Name;
            var receiver = Clients.CallerState.UserName;
            ObservableCollection<MessageModel> messages = new ObservableCollection<MessageModel>();
            foreach (var msg in MessageDb
                .Where(o => o.SenderName == sender || o.SenderName == receiver)
                .Where(o => o.ReceiverName == receiver || o.ReceiverName == sender))
            {
                if (receiver == msg.ReceiverName) //if person logging in is the receiver of the message
                {
                    msg.IsOriginNative = false; // person loggin in is not the author
                }
                else
                {
                    msg.IsOriginNative = true;
                }
                messages.Add(msg);
                await MessageDelivered(msg);
            }

            return messages.ToList();
        }

        public async Task SendMessage(ParticipantModel recepient, MessageModel message)
        {
            var sender = Clients.CallerState.UserName;
            var database = recepient.DatabaseName;
            List<string> senderConnections = ChatClientsOfDb[database][sender].Connections;

            if (!string.IsNullOrEmpty(sender) && recepient.Name != sender && !string.IsNullOrEmpty(message.Message) && ChatClientsOfDb[database].ContainsKey(recepient.Name))
            {
                message.SenderId = Clients.CallerState.Id;
                message.SenderName = Clients.CallerState.UserName;
                message.Status = MessageStatus.Sent;
                message.SentTime = DateTime.Now;
                message.IsOriginNative = false;

                MessageDb.Add(message);

                //

                //await Clients.Group(recepient.Name).MessageReceived(message); // Send message to the connection Ids under the groupname

                await Clients.Clients(recepient.Connections).MessageReceived(message); // send message to all connections associated with recepient
                await Clients.Clients(senderConnections).MessageSent(message);
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