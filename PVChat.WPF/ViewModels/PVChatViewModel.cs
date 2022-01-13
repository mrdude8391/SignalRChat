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
        //public ObservableCollection<MessageModel> SelectedMessages { get; } = new ObservableCollection<MessageModel>();

        public ObservableCollection<ParticipantModel> Participants { get; }

        public ICommand ConnectCommand { get; }
        public ICommand SendMessageCommand { get; }
        public ICommand LogoutCommand { get; }

        private readonly ISignalRChatService _chatService;
        private readonly NavigationService _navService;

        public PVChatViewModel(ISignalRChatService chatService, NavigationService navService, List<ParticipantModel> users)
        {
            _chatService = chatService;
            _navService = navService;
            _isConnected = true;

            SendMessageCommand = new SendMessageCommand(this, _chatService);
            ConnectCommand = new ConnectCommand(this, _chatService);
            LogoutCommand = new LogoutCommand(_chatService);

            _chatService.LoggedIn += OtherUserLoggedIn;
            _chatService.ParticipantLogout += OtherUserLoggedOut;
            _chatService.MessageReceived += MessageReceived;
            _chatService.MessageSent += MessageSent;
            _chatService.MessageDelivered += MessageDelivered;
            _chatService.MessageDeliveredForReceivers += MessageDeliveredForReceivers;
            _chatService.ParticipantsMessageRead += ParticipantsMessageRead;

            Participants = new ObservableCollection<ParticipantModel>(users);

            GetMessages();
        }

        

        private void SetVisibility() // send messages box visibility
        {
            if (SelectedParticipant != null)
                SendMessageVisiblity = true;
            else
                SendMessageVisiblity = false;
        }

        private async void GetMessages() // get messages of selected user
        {
            if (SelectedParticipant != null)
            {
                var user = Participants.Where(u => u.Name == SelectedParticipant.Name).FirstOrDefault();
                user.Unread = false;
                var msgs = await _chatService.GetMessages(user);
                SelectedParticipant.Messages.Clear();
                //SelectedMessages.Clear();
                foreach (var msg in msgs)
                {
                    //SelectedMessages.Add(msg);
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

                if (ReceivedWhileChatOpen(message))
                {
                    message.Unread = false;
                    SelectedParticipant.Messages.Add(message);
                }
                else
                {
                    participant.Unread = true;
                }
            });

            await _chatService.ConfirmMessageDelivered(participant, message);
        }

        private bool ReceivedWhileChatOpen(MessageModel message) => SelectedParticipant != null && SelectedParticipant.Name == message.SenderName; // if message was receieved while chat was open or not

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

        private void ParticipantsMessageRead(ParticipantModel user)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                var participant = Participants.FirstOrDefault(o => o.Name == user.Name);
                participant.Unread = false;

                foreach(var msg in participant.Messages)
                {
                    msg.Unread = false;
                }

            });
        }

        private void MessageDeliveredForReceivers(MessageModel message)
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
                    if(msg.Unread == false)
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
                var p = Participants.FirstOrDefault(o => o.Name == user.Name);
                if (p != null)
                {
                    p.Connections = user.Connections;
                    p.Online = user.Online;
                }
            });
        }
    }
}