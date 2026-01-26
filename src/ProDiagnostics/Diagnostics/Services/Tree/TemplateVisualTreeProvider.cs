using System;
using System.Collections.Generic;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace Avalonia.Diagnostics.Services
{
    internal sealed class TemplateVisualTreeProvider : ITemplateVisualTreeProvider
    {
        public IReadOnlyList<Visual> GetTemplateRoots(Control owner)
        {
            if (owner == null)
            {
                return Array.Empty<Visual>();
            }

            var roots = new List<Visual>();
            foreach (var child in owner.VisualChildren)
            {
                if (ReferenceEquals(child.TemplatedParent, owner))
                {
                    roots.Add(child);
                }
            }

            return roots;
        }

        public bool HasTemplateRoots(Control owner)
        {
            if (owner == null)
            {
                return false;
            }

            foreach (var child in owner.VisualChildren)
            {
                if (ReferenceEquals(child.TemplatedParent, owner))
                {
                    return true;
                }
            }

            return false;
        }

        public IDisposable? SubscribeTemplateRootsChanged(Control owner, Action callback)
        {
            if (owner == null)
            {
                return null;
            }

            void Invoke()
            {
                callback();
            }

            return owner.VisualChildren.ForEachItem(
                (_, _) => Invoke(),
                (_, _) => Invoke(),
                Invoke);
        }
    }
}
