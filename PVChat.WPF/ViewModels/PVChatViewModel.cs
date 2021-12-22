using PVChat.Domain.Models;
using PVChat.Domain.Services;
using PVChat.WPF.Commands;
using PVChat.WPF.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PVChat.WPF.ViewModels
{
    public class PVChatViewModel : ViewModelBase
    {
        private string _errorMessage = string.Empty;

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(HasErrorMessage));
            }
        }

        private bool _isConnected;

        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                OnPropertyChanged(nameof(IsConnected));
            }
        }

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

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);
        public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();
        public ObservableCollection<ParticipantModel> Participants { get; } = new ObservableCollection<ParticipantModel>();

        private ParticipantModel _selectedParticipant;

        public ParticipantModel SelectedParticipant
        {
            get { return _selectedParticipant; }
            set { _selectedParticipant = value; OnPropertyChanged(nameof(SelectedParticipant)); }
        }

        public ICommand ConnectCommand { get; }
        public ICommand SendMessageCommand { get; }
        public ICommand LogoutCommand { get; }

        private readonly ISignalRChatService _chatService;
        private readonly List<UserModel> _users;

        public PVChatViewModel(ISignalRChatService chatService, List<UserModel> Users)
        {
            _chatService = chatService;
            _users = Users;

            SendMessageCommand = new SendMessageCommand(this, chatService);
            ConnectCommand = new ConnectCommand(this, chatService);
            LogoutCommand = new LogoutCommand(chatService);

            Messages = new ObservableCollection<string>();
            Participants = new ObservableCollection<ParticipantModel>();
            Users.ForEach(u => Participants.Add(new ParticipantModel { Name = u.Name }));

            _chatService.BroadcastReceived += BroadcastMessageReceived;
            _chatService.LoggedIn += OtherUserLoggedIn;
            _chatService.ParticipantLogout += OtherUserLoggedOut;
            _chatService.MessageReceived += MessageReceived;
            _chatService.MessageSent += MessageSent;
            _chatService.MessageDelivered += MessageDelivered;
        }

        

        private async void MessageReceived(string sender, MessageModel model)
        {
            App.Current.Dispatcher.Invoke((Action)delegate // <-- LINE
            {
                Messages.Add($"{sender}: {model.Message}");
            });

            await _chatService.ConfirmMessageDelivered(sender, model);
        }

        private void MessageSent(string message)
        {
            App.Current.Dispatcher.Invoke((Action)delegate // <-- LINE
            {
                Messages.Add($"{message}");
            });
        }

        private void MessageDelivered(string confirm)
        {
            App.Current.Dispatcher.Invoke((Action)delegate // <-- LINE
            {
                Messages.Add($"{confirm}");
            });
        }

        private void BroadcastMessageReceived(string message)
        {
            //Might not need this LINE outside of prototype
            //LINE exists because of Thread Affinity, the VM is created in APP.CS so the Messages <observablecollection> is created in a different thread
            //LINE tells it which thread to add to
            App.Current.Dispatcher.Invoke((Action)delegate // <-- LINE
            {
                Messages.Add(message);
            });
        }

        private void OtherUserLoggedIn(UserModel user)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                Messages.Add($"{user.Name} has logged in.");
                Participants.Add(new ParticipantModel
                {
                    Name = user.Name,
                });
            });
        }
        private void OtherUserLoggedOut(UserModel user)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                Messages.Add($"{user.Name} has logged out.");
                Participants.Remove(Participants.Where(o => o.Name == user.Name).SingleOrDefault());
            });
        }
    }
}