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
        event Action<UserModel> LoggedIn;
        event Action<UserModel> ParticipantLogout;
        event Action ConnectionClosed;
        event Action<string> BroadcastReceived;
        event Action<MessageModel> MessageReceived;
        event Action<MessageModel> MessageSent;
        event Action<string> MessageDelivered;

        Task Connect();
        Task Logout();
        Task BroadcastMessage(string message);
        Task SendMessage(string recepient, MessageModel message);
        Task ConfirmMessageDelivered(MessageModel message);
        Task<List<UserModel>> Login(string Name);
        Task<List<MessageModel>> GetMessages(UserModel user);
    }
}
