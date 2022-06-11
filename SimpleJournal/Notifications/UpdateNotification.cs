using SimpleJournal;
using SimpleJournal.Common;
using SimpleJournal.Common.Data;
using SimpleJournal.Dialogs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Notifications
{
    /// <summary>
    /// Notification if a new SimpleJournal Update is avaialbe
    /// </summary>
    public class UpdateNotification : Notification
    {
        private Version version;

        public override string ID => "UPDATE-001";

        [XmlIgnore]
        public override List<Run> Message
        {
            get
            {
                var tempVersion = version;
                if (tempVersion == null)
#if !UWP
                    tempVersion = Consts.NormalVersion; // for debugging purposes
#else
                    tempVersion = Consts.StoreVersion;
#endif

                Run run1 = new Run(SimpleJournal.Properties.Resources.strNotifications_Update_MessageRun1);
                Run run2 = new Run($"Version {tempVersion:4}") { IsBold = true };

                return new List<Run>() { run1, new LineBreak(), new LineBreak(), run2 };
            }
        }

        [XmlIgnore]
        public override List<UserInteraction> UserInteractions
        {
            get
            {
                return new List<UserInteraction>()
                {
                    new UserInteraction()
                    {
                        Description = SimpleJournal.Properties.Resources.strNotifications_UserInteraction_ShowChangelog,
                        HandleUserInteraction = new Action(() => 
                        {
                            AboutDialog aboutDialog = new AboutDialog();
                            aboutDialog.ShowChangelogPage().ShowDialog();
                        })
                    },
                    new UserInteraction()
                    {
                        Description = SimpleJournal.Properties.Resources.strNotifications_Update_UserInteraction_ExecuteUpdate,
                        HandleUserInteraction = new Action(() => 
                        {
#if !UWP         
                            UpdateDialog ud = new UpdateDialog(version);
                            ud.ShowDialog();
#else 
                            try
                            {
                                System.Diagnostics.Process.Start("explorer.exe", "\"ms-windows-store://pdp/?productid=9MV6J44M90N7\"");
                            }
                            catch
                            {
                                // ignore
                            }
#endif
                        })
                    }
                };
            }
        }

        public override NotificationType Type => NotificationType.Info;

        public override bool IsAsyncExecutionRequiredForCheckOccurance => true;

        public override TimeSpan ContinuouslyCheckingInterval
        {
            get
            {
                return TimeSpan.FromHours(1);
            }
        }

        public override async Task<bool?> CheckOccuranceAsync()
        {
            // Without this part the notification would be removed if there is not internet connection
            // and would be added again if the interent connection is back.
            // But since we know there is a new version avaialbe the notification should stay as long as the version gets updated!
            if (!GeneralHelper.IsConnectedToInternet())
                return null;

            version = await GeneralHelper.CheckForUpdatesAsync();
            return (version != null);
        }
    }
}