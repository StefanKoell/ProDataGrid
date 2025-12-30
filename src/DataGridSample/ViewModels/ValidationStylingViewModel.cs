using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Controls;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class ValidationStylingViewModel : ObservableObject
    {
        public ValidationStylingViewModel()
        {
            Legend = new ReadOnlyCollection<ValidationLegendItem>(new[]
            {
                new ValidationLegendItem("Error", DataGridValidationSeverity.Error, "Blocks commit."),
                new ValidationLegendItem("Warning", DataGridValidationSeverity.Warning, "Allows commit and keeps the cell highlighted."),
                new ValidationLegendItem("Info", DataGridValidationSeverity.Info, "Allows commit and keeps the cell highlighted.")
            });

            Items = new ObservableCollection<ValidationStylingItem>
            {
                new ValidationStylingItem { Title = "Launch plan", Risk = 5m, Health = 72m },
                new ValidationStylingItem { Title = "Security audit", Risk = 6m, Health = 68m },
                new ValidationStylingItem { Title = "Docs refresh", Risk = 4m, Health = 80m }
            };
        }

        public ObservableCollection<ValidationStylingItem> Items { get; }

        public IReadOnlyList<ValidationLegendItem> Legend { get; }
    }

    public sealed class ValidationLegendItem
    {
        public ValidationLegendItem(string label, DataGridValidationSeverity severity, string description)
        {
            Label = label;
            Severity = severity;
            Description = description;
        }

        public string Label { get; }

        public DataGridValidationSeverity Severity { get; }

        public string Description { get; }
    }

    public class ValidationStylingItem : ObservableObject, INotifyDataErrorInfo
    {
        private string _title = string.Empty;
        private decimal? _risk;
        private decimal? _health;
        private readonly Dictionary<string, List<DataGridValidationResult>> _errors = new();

        public string Title
        {
            get => _title;
            set
            {
                if (!SetProperty(ref _title, value))
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(value))
                {
                    SetError(nameof(Title),
                        new DataGridValidationResult("Title is required.", DataGridValidationSeverity.Error));
                }
                else
                {
                    SetError(nameof(Title), null);
                }
            }
        }

        public decimal? Risk
        {
            get => _risk;
            set
            {
                if (!SetProperty(ref _risk, value))
                {
                    return;
                }

                if (!value.HasValue)
                {
                    SetError(nameof(Risk),
                        new DataGridValidationResult("Risk is required.", DataGridValidationSeverity.Error));
                }
                else if (value.Value >= 8m)
                {
                    SetError(nameof(Risk),
                        new DataGridValidationResult("Risk of 8+ is a warning.", DataGridValidationSeverity.Warning));
                }
                else
                {
                    SetError(nameof(Risk), null);
                }
            }
        }

        public decimal? Health
        {
            get => _health;
            set
            {
                if (!SetProperty(ref _health, value))
                {
                    return;
                }

                if (!value.HasValue)
                {
                    SetError(nameof(Health),
                        new DataGridValidationResult("Health is required.", DataGridValidationSeverity.Error));
                }
                else if (value.Value < 50m)
                {
                    SetError(nameof(Health),
                        new DataGridValidationResult("Health under 50 is informational.", DataGridValidationSeverity.Info));
                }
                else
                {
                    SetError(nameof(Health), null);
                }
            }
        }

        public bool HasErrors => _errors.Count > 0;

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public IEnumerable GetErrors(string? propertyName)
        {
            if (propertyName is { } && _errors.TryGetValue(propertyName, out var errorList))
            {
                return errorList;
            }

            return Array.Empty<object>();
        }

        private void SetError(string propertyName, DataGridValidationResult? error)
        {
            if (error == null)
            {
                if (_errors.Remove(propertyName))
                {
                    OnErrorsChanged(propertyName);
                }
                return;
            }

            if (_errors.TryGetValue(propertyName, out var errorList))
            {
                errorList.Clear();
                errorList.Add(error);
            }
            else
            {
                _errors.Add(propertyName, new List<DataGridValidationResult> { error });
            }

            OnErrorsChanged(propertyName);
        }

        private void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}
