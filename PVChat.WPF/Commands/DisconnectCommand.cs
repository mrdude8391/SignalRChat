using PVChat.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVChat.WPF.Commands
{
    public class DisconnectCommand : CommandBase
    {
        private readonly ISignalRChatService _chatService;
        public DisconnectCommand(ISignalRChatService chatService)
        {
            _chatService = chatService;
        }
        public override void Execute(object parameter)
        {
            try
            {
                return;
            }
            catch (Exception)
            {

                return;
            }
        }
    }
}
