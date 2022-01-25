using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace PVChat.WPF.Services
{
    public class NotificationService : INotifyPropertyChanged
    {
        private readonly NotifyIcon _notifyIcon;
        private bool _isFocused;

        public bool IsFocused
        {
            get { return _isFocused; }
            set { _isFocused = value; OnPropertyChanged(nameof(IsFocused)); }
        }


        public NotificationService()
        {
            _notifyIcon = new NotifyIcon();
            
            //icon property has to be embedded resource and always copy to output directory
            _notifyIcon.Icon = new Icon("Images/NewMessage.ico");
            _notifyIcon.Visible = true;
            
        }

        public void Notify(string title, string message)
        {
            _notifyIcon.ShowBalloonTip(200, title, message, ToolTipIcon.Info);
        }
        
        public void Close()
        {
            _notifyIcon.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}