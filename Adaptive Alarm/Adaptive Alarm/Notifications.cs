using System;
using System.Collections.Generic;
using System.Text;

namespace Adaptive_Alarm
{
    public interface INotificationManager
    {
        event EventHandler NotificationReceived;
        void Initialize();
        int SendNotification(string title, string message, DateTime? notifyTime = null);
        void ReceiveNotification(string title, string message);

        void updateNotification(string title, string message, DateTime? notifyTime, int ID);
    }

    public class NotificationEventArgs : EventArgs
    {
        public string Title { get; set; }
        public string Message { get; set; }
    }
}
