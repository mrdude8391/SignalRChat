using PVChat.Domain.Models;
using PVChat.Domain.Services;
using PVChat.WPF.Commands;
using PVChat.WPF.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        private UserModel _selectedUser;

        public UserModel SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                _selectedUser = value;
                OnPropertyChanged(nameof(SelectedUser));
                GetMessages();
                SetVisibility();
            }
        }

        private bool _sendMessageVisiblity;

        public bool SendMessageVisiblity
        {
            get { return _sendMessageVisiblity; }
            set
            {
                _sendMessageVisiblity = value;
                OnPropertyChanged(nameof(SendMessageVisiblity));
            }
        }

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);
        public ObservableCollection<MessageModel> Messages { get; } = new ObservableCollection<MessageModel>();
        public ObservableCollection<MessageModel> SelectedMessages { get; } = new ObservableCollection<MessageModel>();

        public ObservableCollection<UserModel> Users { get; }

        public ICommand ConnectCommand { get; }
        public ICommand SendMessageCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand BackCommand { get; }

        private readonly ISignalRChatService _chatService;
        private readonly NavigationService _navService;

        public PVChatViewModel(ISignalRChatService chatService, NavigationService navService, List<UserModel> users)
        {
            _chatService = chatService;
            _navService = navService;

            SendMessageCommand = new SendMessageCommand(this, _chatService);
            ConnectCommand = new ConnectCommand(this, _chatService);
            LogoutCommand = new LogoutCommand(_chatService);

            Messages = new ObservableCollection<MessageModel>();

            _chatService.BroadcastReceived += BroadcastMessageReceived;
            _chatService.LoggedIn += OtherUserLoggedIn;
            _chatService.ParticipantLogout += OtherUserLoggedOut;
            _chatService.MessageReceived += MessageReceived;
            _chatService.MessageSent += MessageSent;
            _chatService.MessageDelivered += MessageDelivered;

            Users = new ObservableCollection<UserModel>(users);
        }

        private void SetVisibility()
        {
            if (SelectedUser != null)
                SendMessageVisiblity = true;
            else
                SendMessageVisiblity = false;
        }

        private async void GetMessages()
        {
            if (SelectedUser != null)
            {
                var user = Users.Where(u => u.Name == SelectedUser.Name).FirstOrDefault();
                user.Unread = false;
                var msgs = await _chatService.GetMessages(user);
                SelectedMessages.Clear();
                foreach (var msg in msgs)
                {
                    SelectedMessages.Add(msg);
                }
            }
        }

        private async void MessageReceived(MessageModel message)
        {
            App.Current.Dispatcher.Invoke((Action)delegate // <-- LINE
            {
                foreach (var user in Users
                .Where(o => o.Id == message.SenderId))
                {
                    if(!receivedWhileChatOpen(message))
                        user.Unread = true;
                }
                if (receivedWhileChatOpen(message))
                {
                    SelectedMessages.Add(message);
                }
                Messages.Add(message);
            });

            await _chatService.ConfirmMessageDelivered(message);
        }

        private bool receivedWhileChatOpen(MessageModel message) => SelectedUser != null && SelectedUser.Id == message.SenderId;

        private void MessageSent(MessageModel message)
        {
            App.Current.Dispatcher.Invoke((Action)delegate // <-- LINE
            {
                foreach (var msg in Messages.Where(o => o.MessageId == message.MessageId))
                {
                    msg.SentTime = message.SentTime;
                    msg.Status = message.Status;
                }
            });
        }

        private void MessageDelivered(string confirm)
        {
            App.Current.Dispatcher.Invoke((Action)delegate // <-- LINE
            {
                //Messages.Add($"{confirm}");
            });
        }

        private void BroadcastMessageReceived(string message)
        {
            //Might not need this LINE outside of prototype
            //LINE exists because of Thread Affinity, the VM is created in APP.CS so the Messages <observablecollection> is created in a different thread
            //LINE tells it which thread to add to
            App.Current.Dispatcher.Invoke((Action)delegate // <-- LINE
            {
                //Messages.Add(message);
            });
        }

        private void OtherUserLoggedIn(UserModel user)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                Users.Add(user);
            });
        }

        private void OtherUserLoggedOut(UserModel user)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                Users.Remove(Users.Where(o => o.Name == user.Name).FirstOrDefault());
            });
        }
    }
}