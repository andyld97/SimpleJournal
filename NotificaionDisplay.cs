using SimpleJournal.Common;
using System.Windows;

namespace SimpleJournal
{
    internal class NotificaionDisplay : UIElement
    {
        private Notification notification;

        public NotificaionDisplay(Notification notification)
        {
            this.notification = notification;
        }
    }
}