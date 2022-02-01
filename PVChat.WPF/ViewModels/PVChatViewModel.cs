﻿using PVChat.Domain.Models;
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
        private byte[] _image;

        public byte[] Image
        {
            get { return _image; }
            set { _image = value;
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

        public int MessageNotifCount { get; set; }

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);
        public ObservableCollection<ParticipantModel> Participants { get; }

        #endregion Properties

        public ICommand ConnectCommand { get; }
        public ICommand SendMessageCommand { get; }
        public ICommand SendImageCommand { get; }
        public ICommand OpenImageCommand { get; }

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
            SendImageCommand = new SendImageCommand(this, _chatService);
            OpenImageCommand = new OpenImageCommand(this);


            _chatService.LoggedIn += OtherUserLoggedIn;
            _chatService.ParticipantLogout += OtherUserLoggedOut;
            _chatService.MessageReceived += MessageReceived;
            _chatService.MessageSent += MessageSent;
            _chatService.MessageDelivered += MessageDelivered;
            _chatService.MessageDeliveredForReceivers += MessageDeliveredForReceivers;
            _chatService.UpdateMessagesReadStatus += UpdateMessagesReadStatus;

            _notifService.Focused += OnFocused;

            Participants = new ObservableCollection<ParticipantModel>(participants);
            SelectedParticipant = Participants.FirstOrDefault();

            GetMessages();
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
                ReadMessagesOfSelectedParticipant();
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

        private async void GetMessages() // Get messages of selected user
        {
            if (SelectedParticipant != null)
            {
                var user = Participants.Where(u => u.Name == SelectedParticipant.Name).FirstOrDefault();
                user.HasUnreadMessages = false;

                var count = user.Messages.Where(m => m.Unread == true && m.IsOriginNative == false).Count();
                _notifService.NotifCount -= count;

                var msgs = await _chatService.GetMessages(user);

                SetDateBreaks(msgs);
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
                        participant.HasUnreadMessages = true;
                    }
                }
            }
        }

        private List<MessageModel> SetDateBreaks(List<MessageModel> msgs)
        {
            DateTime firstDay = new DateTime();
            foreach(var msg in msgs)
            {
                if(msg.CreatedTime.Date != firstDay.Date)
                {
                    msg.HasDateBreak = true;
                    firstDay = msg.CreatedTime.Date;
                }
            }

            return msgs;
        }

        private async void MessageReceived(MessageModel message) // add messages to list of messages when received from other user
        {
            ParticipantModel participant = Participants.FirstOrDefault(o => o.Name == message.SenderName);
            
            App.Current.Dispatcher.Invoke((Action)delegate // <-- LINE
            {
                message.DeliveredTime = DateTime.Now;
                message.Status = MessageStatus.Delivered;

                if (IsParticipantSelected(message) && _notifService.IsFocused) // if the chat is opened on the selected participant while the message is received
                {
                    message.Unread = false;
                    SelectedParticipant.Messages.Add(message);
                }
                else // the chat is either not opened or a different participant is selected
                {
                    _notifService.Notify(message.SenderName, message.Message);
                    participant.Messages.Add(message);
                    participant.HasUnreadMessages = true;
                }
            });

            await _chatService.ConfirmMessageDelivered(participant, message); // tells the sender that message was delivered
        }

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
                participant.HasUnreadMessages = false;

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
                        participant.HasUnreadMessages = false;
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