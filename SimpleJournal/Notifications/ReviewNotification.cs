using SimpleJournal.Common;
using SimpleJournal.Common.Data;
using SimpleJournal.Documents.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleJournal.Notifications
{
    public class ReviewNotification : Notification
    {
        public override string ID => "REVIEW-001";

        public override List<Run> Message => new List<Run>()
        {
            new Run(Properties.Resources.strReviewNotification_Title) { IsBold = true, IsUnderline = true },
            new LineBreak(),
            new LineBreak(),
            new Run(Properties.Resources.strReviewNotification_Message)
        };

        public override TimeSpan ContinuouslyCheckingInterval => TimeSpan.FromSeconds(20);

        public override List<UserInteraction> UserInteractions => new List<UserInteraction>()
        {
            new UserInteraction()
            {
                Description = Properties.Resources.strReviewNotification_RateSimpleJournal,
                HandleUserInteractionAsync = new Func<Task>(() =>
                {
                    GeneralHelper.OpenUri(new Uri(Consts.ReviewStore));

                    Settings.Instance.UserRatedOrClosedNotification = true;
                    Settings.Instance.Save();

                    return Task.CompletedTask;
                }),
                CloseNotification = true
            },
            new UserInteraction()
            {
                Description = Properties.Resources.strReviewNotification_Close,
                HandleUserInteractionAsync = new Func<Task>(() =>
                {
                    Settings.Instance.UserRatedOrClosedNotification = true;
                    Settings.Instance.Save();

                    return Task.CompletedTask;
                }),
                CloseNotification = true
            }
        };          

        public override NotificationType Type => NotificationType.Info;

        public override Task<bool?> CheckOccurrenceAsync(bool isCalledFromTimer)
        {
#if !UWP
            // This notification is only for the UWP version!
            return Task.FromResult((bool?)false);
#endif

            // Will be shown (not initially), but after the first notification check cycle (duration = 1h)
            if (Settings.Instance.UserRatedOrClosedNotification || !isCalledFromTimer)
                return Task.FromResult((bool?)false);

            return Task.FromResult((bool?)true);
        }
    }
}