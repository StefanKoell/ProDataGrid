using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace Avalonia.Diagnostics.Services
{
    internal interface ITemplateVisualTreeProvider
    {
        IReadOnlyList<Visual> GetTemplateRoots(Control owner);

        bool HasTemplateRoots(Control owner);

        IDisposable? SubscribeTemplateRootsChanged(Control owner, Action callback);
    }
}
