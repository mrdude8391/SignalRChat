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

        private byte[] _image;

        public byte[] Image
        {
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChanged(nameof(Image));
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
        public int MessageNotifCount { get; set; }

        #endregion Properties

        #region Commands

        public ICommand ConnectCommand { get; }
        public ICommand SendMessageCommand { get; }
        public ICommand SendImageCommand { get; }
        public ICommand OpenImageCommand { get; }

        #endregion Commands

        private readonly ISignalRChatService _chatService;
        private readonly NotificationService _notifService;

        public PVChatViewModel(ISignalRChatService chatService, NotificationService notifService, List<ParticipantModel> participants, string name)
        {
            _chatService = chatService;
            _notifService = notifService;
            _isConnected = true;
            _name = name;
            Participants = new ObservableCollection<ParticipantModel>(participants);

            SendMessageCommand = new SendMessageCommand(this, _chatService);
            ConnectCommand = new ConnectCommand(this, _chatService);
            SendImageCommand = new SendImageCommand(this, _chatService);
            OpenImageCommand = new OpenImageCommand(this);

            _chatService.LoggedIn += OtherUserLoggedIn;
            _chatService.ParticipantLogout += OtherUserLoggedOut;
            _chatService.MessageReceived += MessageReceived;
            _chatService.MessageSent += MessageSent;
            _chatService.MessageDelivered += MessageDelivered;
            _chatService.UpdateMessagesReadStatus += UpdateMessagesReadStatus;

            _notifService.Focused += OnFocused;

            SelectedParticipant = Participants.FirstOrDefault();

        }

        private async void GetMessages() // Get messages of selected user, called when user clicks on participant
        {
            try
            {
                if (SelectedParticipant != null)
                {
                    var user = Participants.Where(u => u.Name == SelectedParticipant.Name).FirstOrDefault(); // Remove new message icon
                    user.HasUnreadMessages = false;

                    var count = user.Messages.Where(m => m.Unread == true && m.IsOriginNative == false).Count(); // Clear new notification count for the participant
                    _notifService.NotifCount -= count;

                    var msgs = await _chatService.GetMessages(user); // Get messages from the hub - db

                    SetDateBreaks(msgs);

                    // Might need to be changed if local storage is used for efficieny purposes
                    SelectedParticipant.Messages.Clear(); // Clear all old messages and updates with new ones
                    foreach (var msg in msgs)
                    {
                        SelectedParticipant.Messages.Add(msg);
                    }
                }
                else if (SelectedParticipant == null) // If no participant is selected
                {
                    foreach (var participant in Participants)
                    {
                        if (participant.Messages.Any(o => o.Unread == true))
                        {
                            participant.HasUnreadMessages = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
            
        }

        private void MessageSent(MessageModel message) // Sender Calls: Message was sent to the Hub, syncs data for sender
        {
            App.Current.Dispatcher.Invoke((Action)delegate //
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
                else // add message if no message is found (syncing with other connections)
                {
                    message.IsOriginNative = true;
                    participant.Messages.Add(message);
                }
            });
        }

        private void MessageDelivered(MessageModel message) // Sender Calls: Message was delivered to receiver, syncs message data for sender
        {
            App.Current.Dispatcher.Invoke((Action)delegate // <-- LINE
            {
                var participant = Participants.FirstOrDefault(o => o.Name == message.ReceiverName);
                var msg = participant.Messages.Where(m => m.MessageId == message.MessageId).FirstOrDefault(); // get message in list of participants

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

        private async void MessageReceived(MessageModel message) // Receiver Calls: Add message to list of messages from sender 
        {
            ParticipantModel sender = Participants.FirstOrDefault(o => o.Name == message.SenderName);

            App.Current.Dispatcher.Invoke((Action)delegate // <-- LINE
            {
                message.DeliveredTime = DateTime.Now;
                message.Status = MessageStatus.Delivered;

                if (IsParticipantSelected(message) ) // if the chat is opened on the selected participant while the message is received && _notifService.IsFocused
                {
                    message.Unread = false;
                }
                else if(!IsParticipantSelected(message) && _notifService.IsFocused) // the chat is either not opened or a different participant is selected
                {
                    sender.HasUnreadMessages = true;
                }
                else 
                {
                    _notifService.Notify(message.SenderName, message.Message);
                    sender.HasUnreadMessages = true;
                }
                sender.Messages.Add(message);
            });

            await _chatService.ConfirmMessageDelivered(sender, message); // tells the sender that message was delivered
        }

        private bool IsParticipantSelected(MessageModel message) => SelectedParticipant != null && SelectedParticipant.Name == message.SenderName;

        
        private void UpdateMessagesReadStatus(ParticipantModel user) // Sender Calls : The receiver has read the messages, syncs for sender
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                var participant = Participants.FirstOrDefault(o => o.Name == user.Name);

                foreach (var msg in participant.Messages)
                {
                    ReadMessage(msg);
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

        private void OnFocused()
        {
            try
            {
                int notifs = 0;
                foreach (var p in Participants)
                {
                    notifs += p.Messages.Where(m => m.Unread == true).Count();
                }

                _notifService.NotifCount = notifs;
                //ReadMessagesOfSelectedParticipant();
            }
            catch (Exception)
            {
                return;
            }
        }

        private async void ReadMessagesOfSelectedParticipant()
        {
            try
            {
                SelectedParticipant.HasUnreadMessages = false;
                var msgs = await _chatService.GetMessages(SelectedParticipant);

                SetDateBreaks(msgs);
                SelectedParticipant.Messages.Clear();
                foreach (var msg in msgs)
                {
                    SelectedParticipant.Messages.Add(msg);
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        private void SetVisibility() // send messages box visibility
        {
            if (SelectedParticipant != null)
                SendMessageVisiblity = true;
            else
                SendMessageVisiblity = false;
        }

        private List<MessageModel> SetDateBreaks(List<MessageModel> msgs)
        {
            DateTime firstDay = new DateTime();
            foreach (var msg in msgs)
            {
                if (msg.CreatedTime.Date != firstDay.Date)
                {
                    msg.HasDateBreak = true;
                    firstDay = msg.CreatedTime.Date;
                }
            }

            return msgs;
        }

        private MessageModel ReadMessage(MessageModel message)
        {
            message.Unread = false;
            return message;
        }
    }
}