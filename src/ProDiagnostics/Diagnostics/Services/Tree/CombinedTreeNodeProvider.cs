using Avalonia;
using Avalonia.Diagnostics.ViewModels;

namespace Avalonia.Diagnostics.Services
{
    internal sealed class CombinedTreeNodeProvider : ITreeNodeProvider
    {
        private readonly ITemplateVisualTreeProvider _templateProvider;

        public CombinedTreeNodeProvider(ITemplateVisualTreeProvider templateProvider)
        {
            _templateProvider = templateProvider;
        }

        public TreeNode[] Create(AvaloniaObject root)
        {
            return CombinedTreeNode.Create(root, _templateProvider);
        }
    }
}
