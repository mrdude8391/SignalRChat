using PVChat.WPF.Commands;
using PVChat.WPF.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PVChat.WPF.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly NavigationService _navService;
        private readonly SignalRChatService _chatService;

        public ViewModelBase CurrentViewModel => _navService.CurrentViewModel;
        
        public ICommand LogoutCommand { get; }

        public MainViewModel(NavigationService navService, SignalRChatService chatService)
        {
            _navService = navService;
            _chatService = chatService;

            _navService.CurrentViewModelChanged += OnCurrentViewModelChanged;

            LogoutCommand = new LogoutCommand(chatService);
        }

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
    }
}
