namespace PVChat.Domain.Models
{
    public class UserModel : ModelBase
    {
        public string Id { get; set; }
        public string Name { get; set; }
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
    }
}