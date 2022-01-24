using System.Drawing;
using System.Windows.Forms;

namespace PVChat.WPF.Services
{
    public class NotificationService
    {
        private readonly NotifyIcon _notifyIcon;

        public NotificationService()
        {
            _notifyIcon = new NotifyIcon();
            
            //_notifyIcon.Icon = new System.Drawing.Icon("Images/NewMessage.ico");
            //icon property has to be embedded resource and always copy to output directory
            _notifyIcon.Icon = new Icon("Images/NewMessage.ico");
            _notifyIcon.Visible = true;
            
        }

        public void Notify(string title, string message)
        {
            _notifyIcon.ShowBalloonTip(300, title, message, ToolTipIcon.Info);
        }
        
        public void Close()
        {
            _notifyIcon.Dispose();
        }
    }
}