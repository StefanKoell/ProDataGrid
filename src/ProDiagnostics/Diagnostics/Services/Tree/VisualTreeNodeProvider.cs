using Avalonia;
using Avalonia.Diagnostics.ViewModels;

namespace Avalonia.Diagnostics.Services
{
    internal sealed class VisualTreeNodeProvider : ITreeNodeProvider
    {
        public TreeNode[] Create(AvaloniaObject root)
        {
            return VisualTreeNode.Create(root);
        }
    }
}
