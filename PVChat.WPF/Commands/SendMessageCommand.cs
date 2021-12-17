using PVChat.WPF.Services;
using PVChat.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PVChat.WPF.Commands
{
    public class SendMessageCommand : ICommand
    {
        private readonly PVChatViewModel _viewModel;
        private readonly SignalRChatService _chatService;

        public SendMessageCommand(PVChatViewModel viewModel, SignalRChatService chatService)
        {
            _viewModel = viewModel;
            _chatService = chatService;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            try
            {
                var msg = "Bro";
                await _chatService.SendMessage(msg); //_viewModel.Message
                _viewModel.ErrorMessage = string.Empty;
            }
            catch (Exception)
            {

                _viewModel.ErrorMessage = "Unable to send message.";
            }
        }
    }
}
