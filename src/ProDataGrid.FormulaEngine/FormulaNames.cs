// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using System.Collections.Generic;

namespace ProDataGrid.FormulaEngine
{
    public enum FormulaNameChangeKind
    {
        Added,
        Updated,
        Removed,
        Cleared
    }

    public sealed class FormulaNameChangedEventArgs : EventArgs
    {
        public FormulaNameChangedEventArgs(string? name, FormulaNameChangeKind changeKind)
        {
            Name = name;
            ChangeKind = changeKind;
        }

        public string? Name { get; }

        public FormulaNameChangeKind ChangeKind { get; }
    }

    public interface IFormulaNameProvider
    {
        bool TryGetName(string name, out FormulaExpression expression);
    }

    public interface IFormulaNameCollection : IFormulaNameProvider
    {
        IEnumerable<string> Names { get; }

        void SetExpression(string name, FormulaExpression expression);
    }

    public interface IFormulaNameChangeNotifier
    {
        event EventHandler<FormulaNameChangedEventArgs>? NameChanged;
    }

    public sealed class FormulaNameTable : IFormulaNameCollection, IFormulaNameChangeNotifier
    {
        private readonly Dictionary<string, FormulaExpression> _names;

        public event EventHandler<FormulaNameChangedEventArgs>? NameChanged;

        public FormulaNameTable()
        {
            _names = new Dictionary<string, FormulaExpression>(StringComparer.OrdinalIgnoreCase);
        }

        public IEnumerable<string> Names => _names.Keys;

        public void SetExpression(string name, FormulaExpression expression)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var existed = _names.ContainsKey(name);
            _names[name] = expression;
            RaiseChanged(name, existed ? FormulaNameChangeKind.Updated : FormulaNameChangeKind.Added);
        }

        public void SetValue(string name, FormulaValue value)
        {
            SetExpression(name, new FormulaLiteralExpression(value));
        }

        public void SetReference(string name, FormulaReference reference)
        {
            SetExpression(name, new FormulaReferenceExpression(reference));
        }

        public bool Remove(string name)
        {
            var removed = _names.Remove(name);
            if (removed)
            {
                RaiseChanged(name, FormulaNameChangeKind.Removed);
            }
            return removed;
        }

        public void Clear()
        {
            if (_names.Count == 0)
            {
                return;
            }

            _names.Clear();
            RaiseChanged(null, FormulaNameChangeKind.Cleared);
        }

        public bool TryGetName(string name, out FormulaExpression expression)
        {
            return _names.TryGetValue(name, out expression!);
        }

        private void RaiseChanged(string? name, FormulaNameChangeKind changeKind)
        {
            NameChanged?.Invoke(this, new FormulaNameChangedEventArgs(name, changeKind));
        }
    }
}
