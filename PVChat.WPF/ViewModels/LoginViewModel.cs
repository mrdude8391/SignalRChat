using PVChat.WPF.Commands;
using PVChat.WPF.Services;
using System.Windows.Input;

namespace PVChat.WPF.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {

        #region Properties
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
        #endregion

        public ICommand LoginCommand { get; } // COMMANDS NEED TO BE PUBLIC

        private readonly SignalRChatService _chatService;
        private readonly NavigationService _navService;
        private readonly NotificationService _notifService;

        public LoginViewModel(NavigationService navService, SignalRChatService chatService, NotificationService notifService)
        {
            _navService = navService;
            _chatService = chatService;
            _notifService = notifService;

            LoginCommand = new LoginCommand(this, _navService, _chatService, _notifService);

            DatabaseName = "NS_1";
        }
    }
}