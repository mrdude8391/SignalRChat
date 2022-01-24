using PVChat.Domain.Models;
using PVChat.Domain.Services;
using PVChat.WPF.Services;
using PVChat.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PVChat.WPF.Commands
{
    public class LoginCommand : CommandBase
    {
        private readonly LoginViewModel _viewModel;
        private readonly ISignalRChatService _chatService;
        private readonly NavigationService _navService;
        private readonly NotificationService _notifService;
        public List<ParticipantModel> Participants { get; set; } = new List<ParticipantModel>();

        public LoginCommand(LoginViewModel viewModel, NavigationService navService, ISignalRChatService chatService, NotificationService notifService)
        {
            _viewModel = viewModel;
            _chatService = chatService;
            _navService = navService;
            _notifService = notifService;
            
        }

        public override async void Execute(object parameter)
        {
            try
            {
                await _chatService.Connect();
                Participants = await _chatService.Login(_viewModel.Name, _viewModel.DatabaseName);
                _viewModel.ErrorMessage = string.Empty;
                _navService.CurrentViewModel = new PVChatViewModel(_chatService, _notifService, Participants, _viewModel.Name);
            }
            catch (Exception)
            {
                _viewModel.ErrorMessage = "Cannot login";
            }
        }
    }
}