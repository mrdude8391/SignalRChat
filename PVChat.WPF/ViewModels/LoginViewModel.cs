using PVChat.WPF.Commands;
using PVChat.WPF.Services;
using System.Windows.Input;

namespace PVChat.WPF.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly SignalRChatService _chatService;
        private readonly NavigationService _navService;
        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private string _databaseName;

        public string DatabaseName
        {
            get { return _databaseName; }
            set { _databaseName = value; OnPropertyChanged(nameof(DatabaseName)); }
        }

        private string _errorMessage;

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        private bool _isConnected = false;

        public bool IsConnected
        {
            get { return _isConnected; }
            set { _isConnected = value; }
        }

        public ICommand LoginCommand { get; } // COMMANDS NEED TO BE PUBLIC

        public LoginViewModel(NavigationService navService, SignalRChatService chatService)
        {
            _navService = navService;
            _chatService = chatService;
            LoginCommand = new LoginCommand(this, _navService, _chatService);
        }
    }
}