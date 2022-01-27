using PVChat.Domain.Models;
using PVChat.Domain.Services;
using PVChat.WPF.Commands;
using PVChat.WPF.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;

namespace PVChat.WPF.ViewModels
{
    public class PVChatViewModel : ViewModelBase
    {
        #region Properties

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

      

        private ParticipantModel _selectedParticipant;

        public ParticipantModel SelectedParticipant
        {
            get { return _selectedParticipant; }
            set
            {
                _selectedParticipant = value;
                OnPropertyChanged(nameof(SelectedParticipant));
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
        public ObservableCollection<ParticipantModel> Participants { get; }

        #endregion Properties

        public ICommand ConnectCommand { get; }
        public ICommand SendMessageCommand { get; }

        private readonly ISignalRChatService _chatService;
        private readonly NotificationService _notifService;

        public PVChatViewModel(ISignalRChatService chatService, NotificationService notifService, List<ParticipantModel> participants, string name)
        {
            _chatService = chatService;
            _notifService = notifService;
            _isConnected = true;
            _name = name;

            SendMessageCommand = new SendMessageCommand(this, _chatService);
            ConnectCommand = new ConnectCommand(this, _chatService);


            _chatService.LoggedIn += OtherUserLoggedIn;
            _chatService.ParticipantLogout += OtherUserLoggedOut;
            _chatService.MessageReceived += MessageReceived;
            _chatService.MessageSent += MessageSent;
            _chatService.MessageDelivered += MessageDelivered;
            _chatService.MessageDeliveredForReceivers += MessageDeliveredForReceivers;
            _chatService.UpdateMessagesReadStatus += UpdateMessagesReadStatus;

            Participants = new ObservableCollection<ParticipantModel>(participants);
            SelectedParticipant = Participants.FirstOrDefault();

            GetMessages();
        }

        private void SetVisibility() // send messages box visibility
        {
            if (SelectedParticipant != null)
                SendMessageVisiblity = true;
            else
                SendMessageVisiblity = false;
        }

        private async void GetMessages() // Get messages of selected user
        {
            if (SelectedParticipant != null)
            {
                var user = Participants.Where(u => u.Name == SelectedParticipant.Name).FirstOrDefault();
                user.Unread = false;
                var count = user.Messages.Where(m => m.Unread == true).Count();
                _notifService.NotifCount -= count;
                var msgs = await _chatService.GetMessages(user);
                SelectedParticipant.Messages.Clear();
                foreach (var msg in msgs)
                {
                    SelectedParticipant.Messages.Add(msg);
                }
            }
            else if (SelectedParticipant == null)
            {
                foreach (var participant in Participants)
                {
                    if (participant.Messages.Any(o => o.Unread == true))
                    {
                        participant.Unread = true;
                    }
                }
            }
        }

        private async void MessageReceived(MessageModel message) // add messages to list of messages when received from other user
        {
            ParticipantModel participant = Participants.FirstOrDefault(o => o.Name == message.SenderName);
            
            App.Current.Dispatcher.Invoke((Action)delegate // <-- LINE
            {
                message.DeliveredTime = DateTime.Now;
                message.Status = MessageStatus.Delivered;

                if (IsParticipantSelected(message))
                {
                    message.Unread = false;
                    SelectedParticipant.Messages.Add(message);
                    
                }
                else
                {
                    //Notify
                    
                    participant.Messages.Add(message);
                    participant.Unread = true;
                }
                if (_notifService.IsFocused == false)
                {
                    _notifService.Notify(message.SenderName, message.Message);
                }
            });

            await _chatService.ConfirmMessageDelivered(participant, message); // tells the sender that message was delivered
        }

        private bool IsChatMinimized(WindowState windowState) => windowState == WindowState.Minimized;
        private bool IsParticipantSelected(MessageModel message) => SelectedParticipant != null && SelectedParticipant.Name == message.SenderName; // if message was receieved while chat was open or not

        private void MessageSent(MessageModel message) // confirmation when message was sent
        {
            App.Current.Dispatcher.Invoke((Action)delegate // <-- LINE
            {
                var participant = Participants.FirstOrDefault(o => o.Name == message.ReceiverName);
                var msg = participant.Messages.Where(m => m.MessageId == message.MessageId).FirstOrDefault();// get message in list of participants

                if (msg != null) // update message model
                {
                    msg.SenderId = message.SenderId;
                    msg.SenderName = message.SenderName;
                    msg.SentTime = message.SentTime;
                    msg.Status = message.Status;
                }
                else // message was sent from user on another device
                {
                    message.IsOriginNative = true;
                    message.Unread = false;
                    participant.Messages.Add(message);
                }
            });
        }

        private void UpdateMessagesReadStatus(ParticipantModel user)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                var participant = Participants.FirstOrDefault(o => o.Name == user.Name);
                participant.Unread = false;

                foreach (var msg in participant.Messages)
                {
                    msg.Unread = false;
                }
            });
        }

        private void MessageDeliveredForReceivers(MessageModel message) // syncs message to hub when the message is delivered
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                var participant = Participants.FirstOrDefault(o => o.Name == message.SenderName);
                var msg = participant.Messages.Where(m => m.MessageId == message.MessageId).FirstOrDefault();

                if (msg != null) // update message model
                {
                    msg.DeliveredTime = message.DeliveredTime;
                    msg.Status = message.Status;
                    msg.Unread = message.Unread;
                    if (msg.Unread == false) // updates read status if message is read in another client instance
                    {
                        participant.Unread = false;
                    }
                }
                else // message was sent from another device. Adds message incase this client doesn't have it already but it should've been added during "MessageSent" verification
                {
                    message.IsOriginNative = true;
                    participant.Messages.Add(message);
                }
            });
        }

        private void MessageDelivered(MessageModel message) // confirmation when message was delivered
        {
            App.Current.Dispatcher.Invoke((Action)delegate // <-- LINE
            {
                var participant = Participants.FirstOrDefault(o => o.Name == message.ReceiverName);
                var msg = participant.Messages.Where(m => m.MessageId == message.MessageId).FirstOrDefault();// get message in list of participants

                if (msg != null) // update message model
                {
                    msg.DeliveredTime = message.DeliveredTime;
                    msg.Status = message.Status;
                    msg.Unread = message.Unread;
                }
                else // message was sent from another device. Adds message incase this client doesn't have it already but it should've been added during "MessageSent" verification
                {
                    message.IsOriginNative = true;
                    participant.Messages.Add(message);
                }
            });
        }

        private void OtherUserLoggedIn(ParticipantModel user)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                if (!Participants.Any(o => o.Name == user.Name))
                    Participants.Add(user);
                else
                {
                    var p = Participants.FirstOrDefault(o => o.Name == user.Name);
                    p.Connections = user.Connections;
                    p.Online = user.Online;
                }
            });
        }

        private void OtherUserLoggedOut(ParticipantModel user)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                var p = Participants.FirstOrDefault(o => o.Name == user.Name); // Gets the model of participant that logged out
                if (p != null)
                {
                    //Sync connections and online status
                    p.Connections = user.Connections;
                    p.Online = user.Online;
                }
            });
        }
    }
}