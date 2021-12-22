using PVChat.Domain.Services;
using PVChat.WPF.Services;
using PVChat.WPF.ViewModels;
using System;

namespace PVChat.WPF.Commands
{
    public class ConnectCommand : CommandBase
    {
        private readonly PVChatViewModel _viewModel;
        private readonly ISignalRChatService _chatService;

        public ConnectCommand(PVChatViewModel viewModel, ISignalRChatService chatService)
        {
            _viewModel = viewModel;
            _chatService = chatService;
        }

        public override async void Execute(object parameter)
        {
            try
            {
                await _chatService.Connect();
                _viewModel.IsConnected = true;
                _viewModel.ErrorMessage = "Connected";
            }
            catch (Exception)
            {
                _viewModel.ErrorMessage = "Unable to connect to chat hub";
            }
        }
    }
}