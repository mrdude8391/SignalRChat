using PVChat.Domain.Services;
using PVChat.WPF.Services;
using PVChat.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVChat.WPF.Commands
{
    public class LogoutCommand : CommandBase
    {

        private readonly ISignalRChatService _chatService;

        public LogoutCommand(ISignalRChatService chatService)
        {
            _chatService = chatService;

        }

        public override async void Execute(object parameter)
        {
            try
            {
                await _chatService.Logout();
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}
