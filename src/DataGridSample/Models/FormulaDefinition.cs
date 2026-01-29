using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using DataGridSample.Mvvm;
using ProDataGrid.FormulaEngine;
using ProDataGrid.FormulaEngine.Excel;

namespace DataGridSample.Models
{
    public sealed class FormulaDefinition : ObservableObject, INotifyDataErrorInfo
    {
        private static readonly ExcelFormulaParser Parser = new();

        private string _formula;
        private string? _errorMessage;
        private readonly Dictionary<string, List<string>> _errors = new();

        public FormulaDefinition(string name, string formula)
        {
            Name = name;
            _formula = formula ?? string.Empty;
            ValidateFormula();
        }

        public string Name { get; }

        public string Formula
        {
            get => _formula;
            set
            {
                if (!SetProperty(ref _formula, value ?? string.Empty))
                {
                    return;
                }

                ValidateFormula();
            }
        }

        public string? ErrorMessage
        {
            get => _errorMessage;
            private set => SetProperty(ref _errorMessage, value);
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

        private void ValidateFormula()
        {
            var formula = _formula?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(formula))
            {
                SetError(nameof(Formula), null);
                ErrorMessage = null;
                return;
            }

            try
            {
                Parser.Parse(formula, new FormulaParseOptions
                {
                    ReferenceMode = FormulaReferenceMode.A1,
                    AllowLeadingEquals = true
                });

                SetError(nameof(Formula), null);
                ErrorMessage = null;
            }
            catch (FormulaParseException ex)
            {
                var message = $"Invalid formula at position {ex.Position + 1}: {ex.Message}";
                SetError(nameof(Formula), message);
                ErrorMessage = message;
            }
            catch (Exception ex)
            {
                var message = $"Invalid formula: {ex.Message}";
                SetError(nameof(Formula), message);
                ErrorMessage = message;
            }
        }

        private void SetError(string propertyName, string? error)
        {
            var hadErrors = _errors.Count > 0;
            if (string.IsNullOrWhiteSpace(error))
            {
                if (_errors.Remove(propertyName))
                {
                    OnErrorsChanged(propertyName);
                }

                if (hadErrors && _errors.Count == 0)
                {
                    OnPropertyChanged(nameof(HasErrors));
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
                _errors.Add(propertyName, new List<string> { error });
            }

            OnErrorsChanged(propertyName);
            if (!hadErrors)
            {
                OnPropertyChanged(nameof(HasErrors));
            }
        }

        private void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}
