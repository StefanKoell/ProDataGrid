using System;
using System.Collections.ObjectModel;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class MailboxMimicViewModel : ObservableObject
    {
        private const int ItemCount = 5000;
        private string _summary = "Messages: 0";

        public MailboxMimicViewModel()
        {
            Items = new ObservableCollection<MailItem>();
            Populate();
        }

        public ObservableCollection<MailItem> Items { get; }

        public string Summary
        {
            get => _summary;
            private set => SetProperty(ref _summary, value);
        }

        private void Populate()
        {
            Items.Clear();

            var senders = new[]
            {
                "Ava UI",
                "Design Team",
                "Build Pipeline",
                "Product Updates",
                "Docs",
                "QA Reports",
                "Customer Success",
                "Release Notes"
            };

            var subjects = new[]
            {
                "Weekly update",
                "Status check",
                "New feedback",
                "Release plan",
                "Action needed",
                "Follow-up",
                "Meeting summary",
                "Next steps"
            };

            var previews = new[]
            {
                "Here is a quick summary of the latest changes and open items.",
                "Please review the notes and let us know if anything looks off.",
                "We pulled the top issues from the backlog and grouped them.",
                "Draft attached for your review. Feedback is welcome.",
                "Reminder: the milestone closes this Friday.",
                "Thanks for the quick turnaround on the last set of updates.",
                "Sharing the latest metrics and the list of blockers.",
                "See the details below and confirm the schedule."
            };

            var now = DateTime.Now;
            for (var i = 0; i < ItemCount; i++)
            {
                var sender = senders[i % senders.Length];
                var subject = $"{subjects[i % subjects.Length]} #{i + 1}";
                var preview = previews[i % previews.Length];
                var received = now.AddMinutes(-i * 7);

                Items.Add(new MailItem(sender, subject, preview, received)
                {
                    IsUnread = i % 5 == 0,
                    IsFlagged = i % 13 == 0,
                    HasAttachment = i % 9 == 0
                });
            }

            Summary = $"Messages: {Items.Count:n0}";
        }

        public class MailItem : ObservableObject
        {
            private bool _isUnread;
            private bool _isFlagged;
            private bool _hasAttachment;

            public MailItem(string sender, string subject, string preview, DateTime received)
            {
                Sender = sender;
                Subject = subject;
                Preview = preview;
                Received = received;
            }

            public string Sender { get; }

            public string Subject { get; }

            public string Preview { get; }

            public DateTime Received { get; }

            public bool IsUnread
            {
                get => _isUnread;
                set => SetProperty(ref _isUnread, value);
            }

            public bool IsFlagged
            {
                get => _isFlagged;
                set => SetProperty(ref _isFlagged, value);
            }

            public bool HasAttachment
            {
                get => _hasAttachment;
                set => SetProperty(ref _hasAttachment, value);
            }
        }
    }
}
