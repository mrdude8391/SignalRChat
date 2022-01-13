using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PVChat.Domain.Models
{
    public class ParticipantModel : ModelBase
    {
        public string Id { get; set; } //remove
        public string Name { get; set; }

        private bool _online;
        public bool Online
        {
            get { return _online; }
            set { _online = value; OnPropertyChanged(nameof(Online)); }
        }

        private bool _unread;
        public bool Unread
        {
            get { return _unread; }
            set { _unread = value; OnPropertyChanged(nameof(Unread)); }
        }
        public ObservableCollection<MessageModel> Messages { get; set; }

        public List<string> Connections { get; set; }
        public string DatabaseName { get; set; }

        public ParticipantModel()
        {
            Messages = new ObservableCollection<MessageModel>();
            Connections = new List<string>();
        }

        
    }
}