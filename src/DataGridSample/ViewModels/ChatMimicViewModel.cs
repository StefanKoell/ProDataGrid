using System;
using System.Collections.ObjectModel;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class ChatMimicViewModel : ObservableObject
    {
        private const int MessageCount = 2000;
        private string _summary = "Messages: 0";

        public ChatMimicViewModel()
        {
            Messages = new ObservableCollection<ChatMessage>();
            Populate();
        }

        public ObservableCollection<ChatMessage> Messages { get; }

        public string Summary
        {
            get => _summary;
            private set => SetProperty(ref _summary, value);
        }

        private void Populate()
        {
            Messages.Clear();

            var senders = new[] { "Alex", "Jordan", "Casey" };
            var texts = new[]
            {
                "Got the latest build running. Looks good on my end.",
                "Can you review the layout changes before lunch?",
                "I will update the docs and send a draft later today.",
                "Thanks! I will keep an eye on the test results.",
                "We should align on the scope for the next sprint.",
                "Adding a few more samples to cover the edge cases.",
                "Sounds good. Let me know if you hit any blockers.",
                "I will follow up after the stand-up.",
                "Please check the list of open items and confirm priorities.",
                "Quick reminder: we have a demo at 3 PM."
            };

            var start = DateTime.Now.AddHours(-6);
            for (var i = 0; i < MessageCount; i++)
            {
                var isOutgoing = i % 3 == 0;
                var sender = isOutgoing ? "You" : senders[i % senders.Length];
                var text = texts[i % texts.Length];

                if (i % 7 == 0)
                {
                    text = $"{text} Please also include the latest notes from the review.";
                }

                Messages.Add(new ChatMessage(sender, text, start.AddMinutes(i * 3), isOutgoing));
            }

            Summary = $"Messages: {Messages.Count:n0}";
        }

        public class ChatMessage : ObservableObject
        {
            private bool _isOutgoing;
            private string _text;

            public ChatMessage(string sender, string text, DateTime timestamp, bool isOutgoing)
            {
                Sender = sender;
                _text = text;
                Timestamp = timestamp;
                _isOutgoing = isOutgoing;
            }

            public string Sender { get; }

            public DateTime Timestamp { get; }

            public bool IsOutgoing
            {
                get => _isOutgoing;
                set
                {
                    if (SetProperty(ref _isOutgoing, value))
                    {
                        OnPropertyChanged(nameof(IsIncoming));
                    }
                }
            }

            public bool IsIncoming => !_isOutgoing;

            public string Text
            {
                get => _text;
                set => SetProperty(ref _text, value);
            }
        }
    }
}
