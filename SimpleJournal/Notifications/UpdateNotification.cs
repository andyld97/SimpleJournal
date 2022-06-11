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
                    tempVersion = Consts.NormalVersion; // for debugging purposes

                Run run1 = new Run(SimpleJournal.Properties.Resources.strNotifications_Update_MessageRun1);
                Run run2 = new Run($"Version {tempVersion:4}") { IsBold  = true };

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
                            var tempVersion = version;
                            if (tempVersion == null)
                                tempVersion = Consts.NormalVersion; // for debugging purposes!

                            UpdateDialog ud = new UpdateDialog(tempVersion);
                            ud.ShowDialog();
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
#if UWP
                return TimeSpan.Zero;
#endif
                return TimeSpan.FromHours(1);
            }
        }

        public override async Task<bool> CheckOccuranceAsync()
        {
#if UWP
            return false;
#endif

            version = await GeneralHelper.CheckForUpdatesAsync();
            return (version != null);
        }
    }
}