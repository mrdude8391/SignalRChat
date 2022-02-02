using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace PVChat.WPF.Services
{
    public class NotificationService : INotifyPropertyChanged
    {
        private readonly NotifyIcon _notifyIcon;
        public event Action Notified;
        public event Action Focused;

        private bool _isFocused;
        public bool IsFocused
        {
            get { return _isFocused; }
            set { _isFocused = value; OnFocused(); }
        }


        private int _notifCount;
        public int NotifCount
        {
            get { return _notifCount; }
            set { _notifCount = value; OnNotified(); }
        }


        public NotificationService()
        {
            _notifyIcon = new NotifyIcon();
            
            //icon property has to be embedded resource and always copy to output directory
            _notifyIcon.Icon = new Icon("Images/NewMessage.ico");
            _notifyIcon.Visible = true;

            _notifCount = 0;
        }

        public void Notify(string title, string message)
        {
            if (string.IsNullOrEmpty(message)) message = "New Message";
            _notifyIcon.ShowBalloonTip(200, title, message, ToolTipIcon.Info);
        }

        private void OnNotified() {  Notified?.Invoke(); }
        private void OnFocused() { Focused?.Invoke(); }
        public void Close() { _notifyIcon.Dispose(); }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}