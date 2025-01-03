﻿using SimpleJournal;
using SimpleJournal.Common;
using SimpleJournal.Common.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Notifications
{
    /// <summary>
    /// Notification for old .NET Core versions (e.g. .NET 6.0.11)
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

                return [runText1, runText2, runText3];
            }
        }

        [XmlIgnore]
        public override List<UserInteraction> UserInteractions
        {
            get
            {
                return
                [
                    new UserInteraction()
                    {
                        // Same text
                        Description = SimpleJournal.Properties.Resources.strNotifications_Update_UserInteraction_ExecuteUpdate,
                        HandleUserInteractionAsync = new Func<Task>(async () => await GeneralHelper.UpdateNETCoreVersionAsync()),
                    }
                ];
            }
        }

        public override NotificationType Type => NotificationType.Warning;

        public ObsoleteNETVersionNotification()
        { }

        public ObsoleteNETVersionNotification(DateTime timestamp) : base(timestamp)
        { }

        public override Task<bool?> CheckOccurrenceAsync(bool isisCalledFromTimer)
        {
            return Task.FromResult((bool?)(Environment.Version < Consts.CompiledDotnetVersion));
        }
    }  
}