using PVChat.WPF.Commands;
using PVChat.WPF.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PVChat.WPF.ViewModels
{
    public class PVChatViewModel : ViewModelBase
    {
        private string _message;

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        public ObservableCollection<string> Messages { get; }

        private string _errorMessage = string.Empty;

        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(HasErrorMessage));
            }
        }

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        private bool _isConnected;

        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }
            set
            {
                _isConnected = value;
                OnPropertyChanged(nameof(IsConnected));
            }
        }

        public ICommand SendMessageCommand { get; }

        public PVChatViewModel(SignalRChatService chatService)
        {
            SendMessageCommand = new SendMessageCommand(this, chatService);
            Messages = new ObservableCollection<string>{ "A", "B" }; 
            chatService.MessageReceived += ChatserviceMessageReceived;
        }

        public static PVChatViewModel CreatedConnectedViewModel(SignalRChatService chatService)
        {
            PVChatViewModel viewModel = new PVChatViewModel(chatService);
            chatService.Connect().ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    viewModel.ErrorMessage = "Unable to connect to chat hub";
                }
            });
            return viewModel;
        }

        private void ChatserviceMessageReceived(string message)
        {
            Messages.Add(message);
        }
    }
}