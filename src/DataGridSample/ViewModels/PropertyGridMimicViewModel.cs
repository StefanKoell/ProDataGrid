using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Collections;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class PropertyGridMimicViewModel : ObservableObject
    {
        private string _summary = "Properties: 0";

        public PropertyGridMimicViewModel()
        {
            Items = new ObservableCollection<PropertyEntry>(CreateSample());
            View = new DataGridCollectionView(Items);
            View.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(PropertyEntry.Group)));

            var groupCount = Items.Select(item => item.Group).Distinct().Count();
            Summary = $"Groups: {groupCount:n0}  Properties: {Items.Count:n0}";
        }

        public ObservableCollection<PropertyEntry> Items { get; }

        public DataGridCollectionView View { get; }

        public string Summary
        {
            get => _summary;
            private set => SetProperty(ref _summary, value);
        }

        private static IEnumerable<PropertyEntry> CreateSample()
        {
            var themeOptions = new[] { "System", "Light", "Dark" };
            var cultureOptions = new[] { "en-US", "de-DE", "fr-FR", "es-ES" };

            var items = new List<PropertyEntry>
            {
                new("Layout", "Width", PropertyEditorKind.Number) { NumberValue = 1280 },
                new("Layout", "Height", PropertyEditorKind.Number) { NumberValue = 720 },
                new("Layout", "Margin", PropertyEditorKind.Text) { TextValue = "12,12,12,12" },
                new("Layout", "Padding", PropertyEditorKind.Text) { TextValue = "8,8,8,8" },
                new("Appearance", "Title", PropertyEditorKind.Text) { TextValue = "Quarterly Report" },
                new("Appearance", "Theme", PropertyEditorKind.Choice, themeOptions) { SelectedOption = "System" },
                new("Appearance", "Accent Color", PropertyEditorKind.Text) { TextValue = "#3A7BD5" },
                new("Behavior", "Read Only", PropertyEditorKind.Boolean) { BoolValue = false },
                new("Behavior", "Auto Save", PropertyEditorKind.Boolean) { BoolValue = true },
                new("Behavior", "Track Changes", PropertyEditorKind.Boolean) { BoolValue = true },
                new("Data", "Culture", PropertyEditorKind.Choice, cultureOptions) { SelectedOption = "en-US" },
                new("Data", "Max Items", PropertyEditorKind.Number) { NumberValue = 500 },
                new("Data", "Connection", PropertyEditorKind.Text) { TextValue = "localhost:5432" }
            };

            for (var i = 1; i <= 12; i++)
            {
                items.Add(new PropertyEntry("Advanced", $"Rule {i:00}", PropertyEditorKind.Text)
                {
                    TextValue = $"Value {i:00}"
                });
            }

            return items;
        }

        public enum PropertyEditorKind
        {
            Text,
            Number,
            Boolean,
            Choice
        }

        public class PropertyEntry : ObservableObject
        {
            private string _textValue;
            private int _numberValue;
            private bool _boolValue;
            private string _selectedOption;

            public PropertyEntry(string group, string name, PropertyEditorKind kind, IReadOnlyList<string>? options = null)
            {
                Group = group;
                Name = name;
                Kind = kind;
                Options = options ?? Array.Empty<string>();
                _selectedOption = Options.Count > 0 ? Options[0] : string.Empty;
                _textValue = string.Empty;
            }

            public string Group { get; }

            public string Name { get; }

            public PropertyEditorKind Kind { get; }

            public IReadOnlyList<string> Options { get; }

            public bool IsText => Kind == PropertyEditorKind.Text;

            public bool IsNumber => Kind == PropertyEditorKind.Number;

            public bool IsBoolean => Kind == PropertyEditorKind.Boolean;

            public bool IsChoice => Kind == PropertyEditorKind.Choice;

            public string TextValue
            {
                get => _textValue;
                set => SetProperty(ref _textValue, value);
            }

            public int NumberValue
            {
                get => _numberValue;
                set => SetProperty(ref _numberValue, value);
            }

            public bool BoolValue
            {
                get => _boolValue;
                set => SetProperty(ref _boolValue, value);
            }

            public string SelectedOption
            {
                get => _selectedOption;
                set => SetProperty(ref _selectedOption, value);
            }
        }
    }
}
