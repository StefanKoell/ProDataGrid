using Avalonia.Controls.DataGridHierarchical;
using Avalonia.Diagnostics.ViewModels;

namespace Avalonia.Diagnostics.Services
{
    internal sealed class TreeHierarchyModelFactory : ITreeHierarchyModelFactory
    {
        public IHierarchicalModel Create(TreeNode[] roots)
        {
            var options = new HierarchicalOptions
            {
                ChildrenSelector = item => (item as TreeNode)?.Children,
                IsLeafSelector = item => item is TreeNode node && node.Children.Count == 0,
                IsExpandedPropertyPath = nameof(TreeNode.IsExpanded),
                VirtualizeChildren = true,
                AllowExpandToItemSearch = true
            };

            var model = new HierarchicalModel(options);
            model.SetRoots(roots);
            return model;
        }
    }
}
