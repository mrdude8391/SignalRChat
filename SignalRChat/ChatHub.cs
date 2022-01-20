using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using PVChat.Domain.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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
        private static ConcurrentDictionary<string, ParticipantModel> ChatClients = new ConcurrentDictionary<string, ParticipantModel>(); // remove soon
        private static ObservableCollection<MessageModel> MessageDb = new ObservableCollection<MessageModel>();

        

        public async Task<List<ParticipantModel>> Login(string name, string database)
        {

            if (!ChatClientsOfDb.ContainsKey(database))
            {
                var added = ChatClientsOfDb.TryAdd(database, new ConcurrentDictionary<string, ParticipantModel>());
                if (!added)
                    return null; 
            }

            if (!ChatClientsOfDb[database].ContainsKey(name)) // if new user in new database
            {
                Console.WriteLine($"++ {name} logged in");
                Clients.CallerState.UserName = name; // set callerstate username
                Clients.CallerState.Id = Context.ConnectionId; // set callerstate id
                Clients.CallerState.Database = database; //set database

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
                Clients.CallerState.Database = database; //set database
                
                List<ParticipantModel> participants = ChatClientsOfDb[database].Values.ToList(); // get all participants including user logging in
                participants.Remove(participants.Where(p => p.Name == name).FirstOrDefault()); // remove user logging in from list of participants

                List<string> participantConnections = GetAllConnectionIds(participants); // get all connections of participants 

                foreach (var participant in participants) // Populate Participant List with messages
                {
                    participant.Messages = new ObservableCollection<MessageModel>(await GetMessagesOnLogin(participant));
                }

                //Change status of user to online and add connection id 
                if(ChatClientsOfDb[database][name].Online != true) //if user is offline set him to online
                    ChatClientsOfDb[database][name].Online = true;
                if (!ChatClientsOfDb[database][name].Connections.Contains(Context.ConnectionId)) // if the connection id already exists, prevent dupe ids
                    ChatClientsOfDb[database][name].Connections.Add(Context.ConnectionId);

                //

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
            string name = Clients.CallerState.UserName;
            string database = Clients.CallerState.Database;

            List<ParticipantModel> participants = ChatClientsOfDb[database].Values.ToList();

            participants.Remove(participants.Where(p => p.Name == name).FirstOrDefault());

            List<string> connections = GetAllConnectionIds(participants);

            ParticipantModel user = ChatClientsOfDb[database][name];

            if (!string.IsNullOrEmpty(name))
            {
                user.Connections.Remove(Context.ConnectionId);
                if (!user.Connections.Any())
                    user.Online = false;
                //

                Clients.Clients(connections).ParticipantLogout(user);

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
            var selectedParticipant = participant.Name;
            var user = Clients.CallerState.UserName;

            List<string> participantConnections = ChatClientsOfDb[participant.DatabaseName][selectedParticipant].Connections;
            ParticipantModel userModel = ChatClientsOfDb[participant.DatabaseName][user];

            ObservableCollection<MessageModel> messages = new ObservableCollection<MessageModel>();
            foreach (var msg in MessageDb
                .Where(o => o.SenderName == selectedParticipant || o.SenderName == user)
                .Where(o => o.ReceiverName == user || o.ReceiverName == selectedParticipant))
            {
                if (user == msg.ReceiverName) //if the receiver of the message is the user
                {
                    msg.IsOriginNative = false;
                }
                else
                {
                    msg.IsOriginNative = true;
                }
                msg.Unread = false; // user read the message
                messages.Add(msg);
            }

            Clients.Clients(participantConnections).UpdateMessagesReadStatus(userModel);
            return messages.ToList();
        }

        public async Task<List<MessageModel>> GetMessagesOnLogin(ParticipantModel participant) // Gets messages for each participant 
        {
            var participantName = participant.Name;
            var userName = Clients.CallerState.UserName;

            ObservableCollection<MessageModel> messages = new ObservableCollection<MessageModel>();
            foreach (var msg in MessageDb      
                .Where(o => o.SenderName == participantName || o.SenderName == userName)
                .Where(o => o.ReceiverName == userName || o.ReceiverName == participantName)) // gets all messages that the user sent to and received from participant
            {
                if (userName == msg.ReceiverName) //if user is the receiver of the message or which side of the ui to display message
                {
                    msg.IsOriginNative = false; // user is not the author
                    
                }
                else
                {
                    msg.IsOriginNative = true; //user is author
                    msg.Unread = false;
                }
                messages.Add(msg); // add to list of messages

                if(msg.Status != MessageStatus.Delivered) // if message wasnt delivered
                    await ConfirmMessageDelivered(participant, msg); // tell the participant that the message was delivered to the user
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

                await Clients.Clients(recepient.Connections).MessageReceived(message); // send message to all connections associated with recepient
                await Clients.Clients(senderConnections).MessageSent(message); // sync with sender that the message was sent to the database
            }
        }

        public async Task ConfirmMessageDelivered(ParticipantModel sender, MessageModel message) //confirms with sender and receiever that the message was delivered
        {
            List<string> senderConnections = ChatClientsOfDb[sender.DatabaseName][sender.Name].Connections;

            List<string> receiverConnections = ChatClientsOfDb[sender.DatabaseName][message.ReceiverName].Connections;

            if (!string.IsNullOrEmpty(sender.Name))
            {
                foreach (var Message in MessageDb
                    .Where(o => o.MessageId == message.MessageId))
                {
                    Message.DeliveredTime = message.DeliveredTime;
                    Message.Status = message.Status;
                    if (message.Unread == false)
                        Message.Unread = false;

                    message.Unread = Message.Unread; // sync message going back with message in database
                }

                //
                //await Clients.Group(sender).MessageDelivered(message);
                await Clients.Clients(receiverConnections).MessageDeliveredForReceivers(message); // syncs with all other receivers that message was delivered to at least one other user
                await Clients.Clients(senderConnections).MessageDelivered(message); // synchronize with sender that the message was delivered to recepient

            }
        }

    }
}