using SimpleJournal.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SimpleJournal.Common
{
    /// <summary>
    /// Notification data structure for the notification system used in SimpleJournal!
    /// </summary>
    public abstract class Notification
    {
        /// <summary>
        /// A unique id for this notification
        /// </summary>
        public abstract string ID { get; }

        /// <summary>
        /// The text/content which will be displayed in the notification
        /// </summary>
        [XmlIgnore]
        public abstract List<Run> Message { get; }

        /// <summary>
        /// The possible interactions the user can choose between
        /// </summary>
        [XmlIgnore]
        public abstract List<UserInteraction> UserInteractions { get; }

        /// <summary>
        /// Type (Information, Warning, Error) of this notification
        /// </summary>
        public abstract NotificationType Type { get; }

        /// <summary>
        /// The timestamp when this notification is occurred
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// If true <see cref="CheckOccuranceAsync"/> should be called, otherwise <see cref="CheckOccurrence"/>
        /// </summary>
        [XmlIgnore]
        public abstract bool IsAsyncExecutionRequiredForCheckOccurrence { get; }

        /// <summary>
        /// Determines if this notification is checked continuously
        /// </summary>
        [XmlIgnore]
        public bool IsContinuouslyChecked => ContinuouslyCheckingInterval > TimeSpan.Zero;

        /// <summary>
        /// Determines if this notification should be checked continuously or only on startup<br/>
        /// Set to <see cref="TimeSpan.Zero"/> if you don't want it to be checked continuously (default value)
        /// </summary>
        [XmlIgnore]
        public virtual TimeSpan ContinuouslyCheckingInterval { get; } = TimeSpan.Zero;

        /// <summary>
        /// Checks if this notification should be raised
        /// </summary>
        /// <returns>null; if this occurrence shouldn't be doing anything</returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual Task<bool?> CheckOccuranceAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if this notification should be raised
        /// </summary>
        /// <returns>null; if this occurrence shouldn't be doing anything</returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual bool? CheckOccurrence()
        {
            throw new NotImplementedException();
        }

        public Notification()
        {

        }

        public Notification(DateTime timestamp)
        {
            Timestamp = timestamp;
        }

        /// <summary>
        /// Checks if notifications should be raised (all types/sub-classes found in the assembly)
        /// </summary>
        /// <param name="currentNotifications">All previously raised notification (can also be an empty list, but must not be null)</param>
        /// <param name="callingAssembly">The calling assembly is used to determine sub-classes of <see cref="Notification"/> in this assembly</param>
        /// <param name="lastCheckedTimestamp">When did this method was called the last time (Important for continuously checked notifications)</param>
        /// <param name="isCalledFromTimer">If this method is called from a polling timer (Important for continuously checked notifications)</param>
        /// <returns>true; if there were notifications added to the list</returns>
        public static async Task<List<Notification>> CheckNotificationsAsync(List<Notification> currentNotifications, Assembly callingAssembly, DateTime lastCheckedTimestamp, bool isCalledFromTimer)
        {            
            var notifications = callingAssembly.GetTypes().Where(t => typeof(Notification).IsAssignableFrom(t) && !t.IsAbstract).ToList();
            List<Notification> newNotifications = new List<Notification>();
            var now = DateTime.Now;

            foreach (var notification in notifications)
            {
                var instance = (Notification)Activator.CreateInstance(notification);

                // Only check for notifications which are not added to the list
                if (currentNotifications.Any(n => n.ID == instance.ID))
                    continue;

                // OneTime-Notifications can be ignored (if this method was called from a timer) 
                if (isCalledFromTimer && !instance.IsContinuouslyChecked)
                    continue;

                if (instance.IsContinuouslyChecked && lastCheckedTimestamp != DateTime.MinValue)
                {
                    // Check if it should be checked now
                    if (lastCheckedTimestamp.Add(instance.ContinuouslyCheckingInterval) > DateTime.Now)
                        continue;
                }

                bool? hasNotificationOccured = null;
                if (instance.IsAsyncExecutionRequiredForCheckOccurrence)
                    hasNotificationOccured = await instance.CheckOccuranceAsync();
                else
                    hasNotificationOccured = instance.CheckOccurrence();

                if (hasNotificationOccured.HasValue && hasNotificationOccured.Value)
                {
                    instance.Timestamp = now;
                    currentNotifications.Add(instance);
                    newNotifications.Add(instance);

                    System.Diagnostics.Debug.WriteLine($"Found notification: {instance}");
                }
            }

            return newNotifications;
        }

        /// <summary>
        /// Removes all obsolete notifications from the given list
        /// </summary>
        /// <param name="notifications"></param>
        /// <returns>true, if there were more than one notification removed</returns>
        public static async Task<List<Notification>> RemoveObsoleteNotificationsAsync(List<Notification> notifications)
        {
            if (notifications == null || notifications.Count == 0)
                return new List<Notification>();

            List<Notification> toRemove = new List<Notification>();
            foreach (var notification in notifications)
            {
                if (notification.IsAsyncExecutionRequiredForCheckOccurrence)
                {
                    var result = await notification.CheckOccuranceAsync();

                    if (result.HasValue && !result.Value)
                        toRemove.Add(notification);
                }
                else if (!notification.IsAsyncExecutionRequiredForCheckOccurrence)
                {
                    var result = notification.CheckOccurrence();
                    if (result.HasValue && !result.Value)
                        toRemove.Add(notification);
                }
            }

            foreach (var notification in toRemove)
            {
                System.Diagnostics.Debug.WriteLine($"Removed notification: {notification}");
                notifications.Remove(notification);
            }

            return toRemove;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var message in Message)
            {
                if (message.IsNewLine)
                    sb.AppendLine();

                sb.Append(message.Text);
            }

            return $"{ID}: {sb}";
        }
    }

    /// <summary>
    /// Represents an action which the user can execute
    /// </summary>
    public class UserInteraction
    {
        /// <summary>
        /// The description of this interaction which will be displayed in LinkLabels/Buttons
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Determines if async execution is required
        /// </summary>
        public bool IsExecutionAsync { get; set; }

        /// <summary>
        /// The actual "interaction" (but async)
        /// </summary>
        [XmlIgnore]
        public Func<Task> HandleUserInteractionAsync { get; set; }

        /// <summary>
        /// /// The actual "interaction"
        /// </summary>
        [XmlIgnore]
        public Action HandleUserInteraction { get; set; }

        /// <summary>
        /// Executes this actions
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteAsync()
        {
            if (IsExecutionAsync)
                await HandleUserInteractionAsync();
            else
                HandleUserInteraction?.Invoke();
        }    
    }
}