using SimpleJournal;
using SimpleJournal.Common;
using SimpleJournal.Common.Data;
using SimpleJournal.Documents.UI.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Notifications
{
    /// <summary>
    /// Notification for old .NET Core versions (e.g. .NET 6.0.2)
    /// </summary>
    public class ObsoleteNETVersionNotification : Notification
    {
        public override string ID => "NET-001";

        [XmlIgnore]
        public override List<Run> Message
        {
            get
            {
                Run runText1 = new Run(SimpleJournal.Properties.Resources.strObsoleteNETVersionNotification_Message_Run1);
                Run runText2 = new Run($"{System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}") { IsBold = true, IsUnderline = true, IsItalic = true  };
                Run runText3 = new Run(SimpleJournal.Properties.Resources.strObsoleteNETVersionNotification_Message_Run2);

                return new List<Run>() { runText1, runText2, runText3 };
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
                        // Same text
                        Description = SimpleJournal.Properties.Resources.strNotifications_Update_UserInteraction_ExecuteUpdate,
                        IsExecutionAsync = true,
                        HandleUserInteractionAsync = new Func<Task>(async () => await GeneralHelper.UpdateNETCoreVersionAsync()),
                    }
                };
            }
        }

        public override NotificationType Type => NotificationType.Warning;

        public override bool IsAsyncExecutionRequiredForCheckOccurance => false;

        public ObsoleteNETVersionNotification()
        { }

        public ObsoleteNETVersionNotification(DateTime timestamp) : base(timestamp)
        { }

        public override bool? CheckOccurance()
        {
            return Environment.Version < Consts.CompiledDotnetVersion;
        }
    }  
}