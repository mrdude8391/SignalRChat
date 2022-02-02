using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PVChat.Domain.Models
{
    public class ParticipantModel : ModelBase
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string DatabaseName { get; set; }
        public ObservableCollection<MessageModel> Messages { get; set; }
        public List<string> Connections { get; set; }
        private bool _online;
        public bool Online
        {
            get { return _online; }
            set { _online = value; OnPropertyChanged(nameof(Online)); }
        }

        private bool _hasUnreadMessages;
        public bool HasUnreadMessages
        {
            get { return _hasUnreadMessages; }
            set { _hasUnreadMessages = value; OnPropertyChanged(nameof(HasUnreadMessages)); }
        }

        public ParticipantModel()
        {
            Messages = new ObservableCollection<MessageModel>();
            Connections = new List<string>();
        }
    }
}