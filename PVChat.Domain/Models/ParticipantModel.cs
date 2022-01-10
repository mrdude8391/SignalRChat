using System.Collections.ObjectModel;

namespace PVChat.Domain.Models
{
    public class ParticipantModel : ModelBase
    {
        public string Id { get; set; }
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
            set
            {
                _unread = value;
                OnPropertyChanged(nameof(Unread));
            }
        }
        private ObservableCollection<MessageModel> _messages;

        public ObservableCollection<MessageModel> Messages
        {
            get { return _messages; }
            set { _messages = value; }
        }

        public ParticipantModel()
        {
            _messages = new ObservableCollection<MessageModel>();
        }

    }
}