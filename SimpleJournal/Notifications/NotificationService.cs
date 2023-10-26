using SimpleJournal;
using SimpleJournal.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Notifications
{
    public class NotificationService
    {
        private System.Threading.Timer noticationTimer;
        private DateTime lastTick = DateTime.MinValue;

        public delegate void onNotificationAdded(Notification notification);
        public delegate void onNotificationRemoved(Notification notification);
        private readonly Type[] additionalTypes;

        public static NotificationService NotificationServiceInstance { get; set; }

        /// <summary>
        /// Called when a new notification is occurred
        /// </summary>
        public event onNotificationAdded OnNotificationAdded;

        /// <summary>
        /// Called when a notification was removed by the system
        /// </summary>
        public event onNotificationRemoved OnNotificationRemoved;

        /// <summary>
        /// The list of all current notifications
        /// </summary>
        public List<Notification> Notifications { get; set; } = new List<Notification>();

        /// <summary>
        /// Determines whether the service is active or not
        /// </summary>
        public bool IsRunning { get; private set; }

        public async void NotificationTimer_Tick(object state)
        {
            var now = DateTime.Now;
            bool isNotFirstCall = (lastTick != DateTime.MinValue);

            var addedNotifications = await Notification.CheckNotificationsAsync(Notifications, typeof(NotificationService).Assembly, lastTick, isNotFirstCall);
            addedNotifications?.ForEach(n => OnNotificationAdded?.Invoke(n));

            var removedNotifications = await Notification.RemoveObsoleteNotificationsAsync(Notifications, isNotFirstCall);
            removedNotifications?.ForEach(n =>
            {
                Notifications.Remove(n);
                OnNotificationRemoved?.Invoke(n);
            });

            if (removedNotifications.Count > 0 || addedNotifications.Count > 0)
            {
                try
                {
                    Serialization.Save(Consts.NotificationsFilePath, Notifications, additionalTypes: additionalTypes);
                }
                catch
                {
                    // ignore
                }
            }

            lastTick = now;
        }

        public void RemoveNotification(Notification notification)
        {
            Notifications.Remove(notification);
            OnNotificationRemoved?.Invoke(notification);
        }

        public NotificationService()
        {
            additionalTypes = typeof(NotificationService).Assembly.GetTypes().Where(t => typeof(Notification).IsAssignableFrom(t) && !t.IsAbstract).ToArray();

            try
            {
                if (System.IO.File.Exists(Consts.NotificationsFilePath))
                {
                    Notifications = Serialization.Read<List<Notification>>(Consts.NotificationsFilePath, additionalTypes: additionalTypes);
                    Notifications ??= new List<Notification>();
                }
            }
            catch
            {
                Notifications = new List<Notification>();
            }
        }
        /// <summary>
        /// Starts the notification service
        /// </summary>
        public void Start()
        {
            IsRunning = true;
            System.Diagnostics.Debug.WriteLine("Starting notification service ...");
            noticationTimer = new System.Threading.Timer(new System.Threading.TimerCallback(NotificationTimer_Tick), null, 0, Convert.ToInt32(Consts.NotificationServiceInterval.TotalMilliseconds));
        }

        /// <summary>
        /// Stops the notification service
        /// </summary>
        public void Stop()
        {
            IsRunning = false;
            System.Diagnostics.Debug.WriteLine("Stopping notification service ...");
            noticationTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}