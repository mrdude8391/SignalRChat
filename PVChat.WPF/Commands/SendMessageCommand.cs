using PVChat.Domain.Models;
using PVChat.Domain.Services;
using PVChat.WPF.Services;
using PVChat.WPF.ViewModels;
using System;

namespace PVChat.WPF.Commands
{
    public class SendMessageCommand : CommandBase
    {
        private readonly PVChatViewModel _viewModel;
        private readonly ISignalRChatService _chatService;

        public SendMessageCommand(PVChatViewModel viewModel, ISignalRChatService chatService)
        {
            _viewModel = viewModel;
            _chatService = chatService;
        }

        public override async void Execute(object parameter)
        {
            try
            {
                //await _chatService.BroadcastMessage(_viewModel.Message); //_viewModel.Message
                await _chatService.SendMessage(_viewModel.SelectedParticipant.Name, _viewModel.Message);
                _viewModel.ErrorMessage = string.Empty;
                _viewModel.Message = string.Empty;
            }
            catch (Exception)
            {
                _viewModel.ErrorMessage = "Unable to send message.";
            }
        }
    }
}