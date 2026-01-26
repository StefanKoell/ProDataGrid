using System;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Diagnostics.Services;

namespace Avalonia.Diagnostics.ViewModels
{
    internal sealed class TemplateTreeNode : TreeNode
    {
        public TemplateTreeNode(Control owner, TreeNode parent, ITemplateVisualTreeProvider templateProvider)
            : base(owner, parent, "/template/", showDecorations: false)
        {
            Children = new TemplateTreeNodeCollection(this, owner, templateProvider);
        }

        public override TreeNodeCollection Children { get; }

        private sealed class TemplateTreeNodeCollection : TreeNodeCollection
        {
            private readonly Control _owner;
            private readonly ITemplateVisualTreeProvider _templateProvider;
            private IDisposable? _subscription;

            public TemplateTreeNodeCollection(TreeNode owner, Control control, ITemplateVisualTreeProvider templateProvider)
                : base(owner)
            {
                _owner = control;
                _templateProvider = templateProvider;
            }

            public override void Dispose()
            {
                _subscription?.Dispose();
                base.Dispose();
            }

            protected override void Initialize(AvaloniaList<TreeNode> nodes)
            {
                RefreshNodes(nodes);
                _subscription = _templateProvider.SubscribeTemplateRootsChanged(_owner, () => RefreshNodes(nodes));
            }

            private void RefreshNodes(AvaloniaList<TreeNode> nodes)
            {
                DisposeNodes(nodes);
                nodes.Clear();

                foreach (var visual in _templateProvider.GetTemplateRoots(_owner))
                {
                    nodes.Add(new VisualTreeNode(visual, Owner));
                }
            }

            private static void DisposeNodes(AvaloniaList<TreeNode> nodes)
            {
                for (var i = 0; i < nodes.Count; i++)
                {
                    nodes[i].Dispose();
                }
            }
        }
    }
}
