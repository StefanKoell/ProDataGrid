using Avalonia;
using Avalonia.Diagnostics.ViewModels;

namespace Avalonia.Diagnostics.Services
{
    internal interface ITreeNodeProvider
    {
        TreeNode[] Create(AvaloniaObject root);
    }
}
