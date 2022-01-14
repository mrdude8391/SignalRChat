using PVChat.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVChat.Domain.Services
{
    public interface ISignalRChatService
    {
        event Action<ParticipantModel> LoggedIn;
        event Action<ParticipantModel> ParticipantLogout;
        event Action ConnectionClosed;
        event Action<MessageModel> MessageReceived;
        event Action<MessageModel> MessageSent;
        event Action<MessageModel> MessageDelivered;
        event Action<MessageModel> MessageDeliveredForReceivers;
        event Action<ParticipantModel> ParticipantsMessageRead;

        Task Connect();
        Task Logout();
        Task SendMessage(ParticipantModel recepient, MessageModel message);
        Task ConfirmMessageDelivered(ParticipantModel sender, MessageModel message);
        Task<List<ParticipantModel>> Login(string Name, string Database);
        Task<List<MessageModel>> GetMessages(ParticipantModel user);
       
    }
}
