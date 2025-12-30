#nullable disable

using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.PropertyStore;
using Avalonia.Reactive;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Avalonia.Controls.Utils
{
#if !DATAGRID_INTERNAL
public
#else
internal
#endif
    interface ICellEditBinding
    {
        bool IsValid { get; }
        IEnumerable<Exception> ValidationErrors { get; }
        IObservable<bool> ValidationChanged { get; }
        bool CommitEdit();
    }

    internal class CellEditBinding : ICellEditBinding
    {
        private readonly LightweightSubject<bool> _changedSubject = new();
        private readonly List<Exception> _validationErrors = new List<Exception>();
        private readonly SubjectWrapper _inner;
        private readonly IValueEntry _valueEntry;
        private readonly Func<object> _getTargetValue;
        private DataGridValidationSeverity _validationSeverity = DataGridValidationSeverity.None;

        public bool IsValid => _validationSeverity != DataGridValidationSeverity.Error;
        public IEnumerable<Exception> ValidationErrors => _validationErrors;
        public IObservable<bool> ValidationChanged => _changedSubject;
        public IAvaloniaSubject<object> InternalSubject => _inner;

        public CellEditBinding(IAvaloniaSubject<object> bindingSourceSubject, IValueEntry valueEntry = null, Func<object> getTargetValue = null)
        {
            _inner = new SubjectWrapper(bindingSourceSubject, this);
            _valueEntry = valueEntry;
            _getTargetValue = getTargetValue;
        }

        private void AlterValidationErrors(Action<List<Exception>> action)
        {
            var hadErrors = _validationErrors.Count > 0;

            action(_validationErrors);
            _validationSeverity = _validationErrors.Count == 0
                ? DataGridValidationSeverity.None
                : ValidationUtil.GetValidationSeverity(_validationErrors);
            var hasErrors = _validationErrors.Count > 0;

            if (hadErrors || hasErrors)
            {
                _changedSubject.OnNext(IsValid);
            }
        }

        public bool CommitEdit()
        {
            _inner.CommitEdit(_getTargetValue);
            UpdateValidationErrorsFromEntry();
            return IsValid;
        }

        private void UpdateValidationErrorsFromEntry()
        {
            if (_valueEntry == null)
            {
                return;
            }

            _valueEntry.GetDataValidationState(out var state, out var error);
            if ((state & BindingValueType.HasError) != 0 && error != null)
            {
                AlterValidationErrors(errors =>
                {
                    errors.Clear();
                    errors.AddRange(ValidationUtil.UnpackException(error));
                });
            }
            else
            {
                AlterValidationErrors(errors => errors.Clear());
            }
        }

        class SubjectWrapper : LightweightObservableBase<object>, IAvaloniaSubject<object>, IDisposable
        {
            private readonly IAvaloniaSubject<object> _sourceSubject;
            private readonly CellEditBinding _editBinding;
            private IDisposable _subscription;
            private object _controlValue;
            private bool _isControlValueSet = false;
            private bool _settingSourceValue = false;

            public SubjectWrapper(IAvaloniaSubject<object> bindingSourceSubject, CellEditBinding editBinding)
            {
                _sourceSubject = bindingSourceSubject;
                _editBinding = editBinding;
            }

            private void SetSourceValue(object value)
            {
                if (!_settingSourceValue)
                {
                    _settingSourceValue = true;

                    try
                    {
                        _sourceSubject.OnNext(value);
                    }
                    catch (Exception ex)
                    {
                        _editBinding.AlterValidationErrors(errors =>
                        {
                            errors.Clear();
                            errors.AddRange(ValidationUtil.UnpackException(ex));
                        });
                    }

                    _settingSourceValue = false;
                }
            }
            private void SetControlValue(object value)
            {
                PublishNext(value);
            }

            private void OnValidationError(BindingNotification notification)
            {
                if (notification.HasValue && IsDataValidationException(notification.Error))
                {
                    SetControlValue(notification.Value);
                }

                if (notification.Error != null)
                {
                    _editBinding.AlterValidationErrors(errors =>
                    {
                        errors.Clear();
                        var unpackedErrors = ValidationUtil.UnpackException(notification.Error);
                        if (unpackedErrors != null)
                            errors.AddRange(unpackedErrors);
                    });
                }
            }

            private static bool IsDataValidationException(Exception exception)
            {
                if (exception is DataValidationException)
                {
                    return true;
                }

                if (exception is AggregateException aggregate)
                {
                    foreach (var inner in aggregate.InnerExceptions)
                    {
                        if (inner is DataValidationException)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            private void OnControlValueUpdated(object value)
            {
                _controlValue = value;
                _isControlValueSet = true;

                if (!_editBinding.IsValid)
                {
                    SetSourceValue(value);
                }
            }
            private void OnSourceValueUpdated(object value)
            {
                void OnValidValue(object val)
                {
                    SetControlValue(val);
                    _editBinding.AlterValidationErrors(errors => errors.Clear());
                }

                if (value is BindingNotification notification)
                {
                    if (notification.ErrorType != BindingErrorType.None)
                        OnValidationError(notification);
                    else
                        OnValidValue(value);
                }
                else
                {
                    OnValidValue(value);
                }
            }

            protected override void Deinitialize()
            {
                _subscription?.Dispose();
                _subscription = null;
            }
            protected override void Initialize()
            {
                _subscription = _sourceSubject.Subscribe(OnSourceValueUpdated);
            }

            void IObserver<object>.OnCompleted()
            {
                throw new NotImplementedException();
            }
            void IObserver<object>.OnError(Exception error)
            {
                throw new NotImplementedException();
            }
            void IObserver<object>.OnNext(object value)
            {
                OnControlValueUpdated(value);
            }

            public void Dispose()
            {
                _subscription?.Dispose();
                _subscription = null;
            }
            public void CommitEdit(Func<object> getValue)
            {
                if (_isControlValueSet)
                {
                    SetSourceValue(_controlValue);
                }
                else if (getValue != null)
                {
                    SetSourceValue(getValue());
                }
            }
        }
    }

    internal class ExplicitCellEditBinding : ICellEditBinding
    {
        private readonly AvaloniaObject _target;
        private readonly AvaloniaProperty _property;
        private readonly LightweightSubject<bool> _changedSubject = new();
        private readonly List<Exception> _validationErrors = new List<Exception>();
        private DataGridValidationSeverity _validationSeverity = DataGridValidationSeverity.None;

        public ExplicitCellEditBinding(AvaloniaObject target, AvaloniaProperty property)
        {
            _target = target;
            _property = property;
        }

        public bool IsValid => _validationSeverity != DataGridValidationSeverity.Error;
        public IEnumerable<Exception> ValidationErrors => _validationErrors;
        public IObservable<bool> ValidationChanged => _changedSubject;

        public bool CommitEdit()
        {
            Exception commitError = null;
            var expression = BindingOperations.GetBindingExpressionBase(_target, _property);
            if (expression != null)
            {
                try
                {
                    expression.UpdateSource();
                }
                catch (Exception ex)
                {
                    commitError = ex;
                }
            }

            if (commitError != null)
            {
                UpdateValidationErrorsFromException(commitError);
            }
            else
            {
                UpdateValidationErrorsFromTarget(expression);

                if (!IsValid && expression is IValueEntry valueEntry && TryGetSourceValue(valueEntry, out var sourceValue))
                {
                    var targetValue = _target.GetValue(_property);
                    if (Equals(targetValue, sourceValue))
                    {
                        expression?.UpdateTarget();
                        UpdateValidationErrorsFromTarget(expression);
                    }
                }
            }

            return IsValid;
        }

        private void UpdateValidationErrorsFromException(Exception error)
        {
            AlterValidationErrors(errors =>
            {
                errors.Clear();
                errors.AddRange(ValidationUtil.UnpackException(error));
            });
        }

        private void UpdateValidationErrorsFromTarget(BindingExpressionBase expression)
        {
            var valueEntry = expression as IValueEntry;
            if (valueEntry != null)
            {
                valueEntry.GetDataValidationState(out var state, out var error);
                if ((state & BindingValueType.HasError) != 0 && error != null)
                {
                    AlterValidationErrors(list =>
                    {
                        list.Clear();
                        list.AddRange(ValidationUtil.UnpackException(error));
                    });
                }
                else
                {
                    AlterValidationErrors(list => list.Clear());
                }

                return;
            }

            if (_target is not Control control)
            {
                AlterValidationErrors(list => list.Clear());
                return;
            }

            var errors = DataValidationErrors.GetErrors(control);
            AlterValidationErrors(list =>
            {
                list.Clear();
                if (errors == null)
                {
                    return;
                }

                foreach (var error in errors)
                {
                    if (error == null)
                    {
                        continue;
                    }

                    if (error is Exception exception)
                    {
                        list.AddRange(ValidationUtil.UnpackException(exception));
                    }
                    else
                    {
                        list.Add(new DataValidationException(error));
                    }
                }
            });
        }

        private void AlterValidationErrors(Action<List<Exception>> action)
        {
            var hadErrors = _validationErrors.Count > 0;

            action(_validationErrors);
            _validationSeverity = _validationErrors.Count == 0
                ? DataGridValidationSeverity.None
                : ValidationUtil.GetValidationSeverity(_validationErrors);
            var hasErrors = _validationErrors.Count > 0;

            if (hadErrors || hasErrors)
            {
                _changedSubject.OnNext(IsValid);
            }
        }

        private static bool TryGetSourceValue(IValueEntry valueEntry, out object sourceValue)
        {
            sourceValue = null;

            try
            {
                if (!valueEntry.HasValue())
                {
                    return false;
                }

                sourceValue = valueEntry.GetValue();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
