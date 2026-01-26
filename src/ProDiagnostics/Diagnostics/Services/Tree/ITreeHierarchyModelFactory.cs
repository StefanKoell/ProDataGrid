using Avalonia.Controls.DataGridHierarchical;
using Avalonia.Diagnostics.ViewModels;

namespace Avalonia.Diagnostics.Services
{
    internal interface ITreeHierarchyModelFactory
    {
        IHierarchicalModel Create(TreeNode[] roots);
    }
}
