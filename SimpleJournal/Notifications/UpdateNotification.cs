using Helper;
using SimpleJournal;
using SimpleJournal.Common;
using SimpleJournal.Common.Data;
#if !UWP         
using SimpleJournal.Dialogs;
#endif
using SimpleJournal.Documents.UI;
using SimpleJournal.Modules;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Consts = SimpleJournal.Consts;

namespace Notifications
{
    /// <summary>
    /// Notification if a new SimpleJournal Update is available
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
                        HandleUserInteractionAsync = new Func<Task>(() => 
                        {
                            AboutModule aboutDialog = new AboutModule();
                            (aboutDialog.ShowChangelogPage() as ITabbedModule).ShowModuleWindow(Settings.Instance.UseModernDialogs, null);

                            return Task.CompletedTask;
                        })
                    },
                    new UserInteraction()
                    {
                        Description = SimpleJournal.Properties.Resources.strNotifications_Update_UserInteraction_ExecuteUpdate,
                        HandleUserInteractionAsync = new Func<Task>(() => 
                        {
#if !UWP         
                            UpdateDialog ud = new UpdateDialog(version, UpdateHelper.GetLastHash());
                            ud.ShowDialog();
#else 
                            GeneralHelper.OpenUri(new Uri("ms-windows-store://pdp/?productid=9MV6J44M90N7"));
#endif

                            return Task.CompletedTask;
                        })
                    }
                };
            }
        }

        public override NotificationType Type => NotificationType.Info;

        public override TimeSpan ContinuouslyCheckingInterval => TimeSpan.FromHours(1);

        public override async Task<bool?> CheckOccurrenceAsync(bool isCalledFromTimer)
        {
            // Without this part the notification would be removed if there is not Internet connection
            // and would be added again if the Internet connection is back.
            // But since we know there is a new version available the notification should stay as long as the version gets updated!
            if (!GeneralHelper.IsConnectedToInternet())
                return null;

            var result = await UpdateHelper.CheckForUpdatesAsync();
            if (result.Result == UpdateResult.Unknown)
                return null;
            else if (result.Result == UpdateResult.UpdateAvailable)
            {
                version = result.Version;
                return true;
            }

            return false;
        }
    }
}