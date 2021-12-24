using PVChat.Domain.Models;
using PVChat.Domain.Services;
using PVChat.WPF.Services;
using PVChat.WPF.ViewModels;
using System;
using System.Collections.Generic;

namespace PVChat.WPF.Commands
{
    public class LoginCommand : CommandBase
    {
        private readonly LoginViewModel _viewModel;
        private readonly ISignalRChatService _chatService;
        private readonly NavigationService _navService;
        public List<UserModel> Users { get; set; } = new List<UserModel>();

        public LoginCommand(LoginViewModel viewModel, NavigationService navService, ISignalRChatService chatService)
        {
            _viewModel = viewModel;
            _chatService = chatService;
            _navService = navService;

        }

        public override async void Execute(object parameter)
        {
            try
            {
                await _chatService.Connect();
                Users = await _chatService.Login(_viewModel.Name);
                _viewModel.ErrorMessage = string.Empty;
                //_navService.CurrentViewModel = new ContactViewModel(_chatService, _navService, Users);
                _navService.CurrentViewModel = new PVChatViewModel(_chatService, _navService, Users);
            }
            catch (Exception)
            {
                _viewModel.ErrorMessage = "Cannot login";
            }
        }
    }
}