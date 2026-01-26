using Avalonia;
using Avalonia.Diagnostics.ViewModels;

namespace Avalonia.Diagnostics.Services
{
    internal sealed class LogicalTreeNodeProvider : ITreeNodeProvider
    {
        public TreeNode[] Create(AvaloniaObject root)
        {
            return LogicalTreeNode.Create(root);
        }
    }
}
