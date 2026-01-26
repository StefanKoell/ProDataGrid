using System;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Reactive;
using Avalonia.VisualTree;
using Avalonia.Diagnostics.Services;

namespace Avalonia.Diagnostics.ViewModels
{
    internal sealed class CombinedTreeNode : TreeNode
    {
        private readonly ITemplateVisualTreeProvider _templateProvider;

        public CombinedTreeNode(AvaloniaObject avaloniaObject, TreeNode? parent, ITemplateVisualTreeProvider templateProvider)
            : base(avaloniaObject, parent)
        {
            _templateProvider = templateProvider;

            Children = avaloniaObject switch
            {
                ILogical logical => new CombinedTreeNodeCollection(this, logical, templateProvider),
                Controls.TopLevelGroup host => new TopLevelGroupHostCombined(this, host, templateProvider),
                _ => TreeNodeCollection.Empty
            };
        }

        public override TreeNodeCollection Children { get; }

        public static CombinedTreeNode[] Create(object control, ITemplateVisualTreeProvider templateProvider)
        {
            var logical = control as AvaloniaObject;
            return logical != null ? new[] { new CombinedTreeNode(logical, null, templateProvider) } : Array.Empty<CombinedTreeNode>();
        }

        private sealed class CombinedTreeNodeCollection : TreeNodeCollection
        {
            private readonly ILogical _control;
            private readonly ITemplateVisualTreeProvider _templateProvider;
            private IDisposable? _logicalSubscription;
            private IDisposable? _templateSubscription;
            private TemplateTreeNode? _templateNode;

            public CombinedTreeNodeCollection(TreeNode owner, ILogical control, ITemplateVisualTreeProvider templateProvider)
                : base(owner)
            {
                _control = control;
                _templateProvider = templateProvider;
            }

            public override void Dispose()
            {
                _logicalSubscription?.Dispose();
                _templateSubscription?.Dispose();
                base.Dispose();
            }

            protected override void Initialize(AvaloniaList<TreeNode> nodes)
            {
                _logicalSubscription = _control.LogicalChildren.ForEachItem(
                    (i, item) => nodes.Insert(i, new CombinedTreeNode((AvaloniaObject)item, Owner, _templateProvider)),
                    (i, item) => nodes.RemoveAt(i),
                    () => ResetNodes(nodes));

                UpdateTemplateNode(nodes);
                if (_control is Control control)
                {
                    _templateSubscription = _templateProvider.SubscribeTemplateRootsChanged(control, () => UpdateTemplateNode(nodes));
                }
            }

            private void ResetNodes(AvaloniaList<TreeNode> nodes)
            {
                if (_templateNode != null)
                {
                    _templateNode.Dispose();
                    _templateNode = null;
                }

                nodes.Clear();
                UpdateTemplateNode(nodes);
            }

            private void UpdateTemplateNode(AvaloniaList<TreeNode> nodes)
            {
                if (_control is not Control control)
                {
                    RemoveTemplateNode(nodes);
                    return;
                }

                if (_templateProvider.HasTemplateRoots(control))
                {
                    if (_templateNode == null)
                    {
                        _templateNode = new TemplateTreeNode(control, Owner, _templateProvider);
                        nodes.Add(_templateNode);
                    }
                }
                else
                {
                    RemoveTemplateNode(nodes);
                }
            }

            private void RemoveTemplateNode(AvaloniaList<TreeNode> nodes)
            {
                if (_templateNode == null)
                {
                    return;
                }

                nodes.Remove(_templateNode);
                _templateNode.Dispose();
                _templateNode = null;
            }
        }

        private sealed class TopLevelGroupHostCombined : TreeNodeCollection
        {
            private readonly Controls.TopLevelGroup _group;
            private readonly CompositeDisposable _subscriptions = new(1);
            private readonly ITemplateVisualTreeProvider _templateProvider;

            public TopLevelGroupHostCombined(TreeNode owner, Controls.TopLevelGroup host, ITemplateVisualTreeProvider templateProvider)
                : base(owner)
            {
                _group = host;
                _templateProvider = templateProvider;
            }

            protected override void Initialize(AvaloniaList<TreeNode> nodes)
            {
                for (var i = 0; i < _group.Items.Count; i++)
                {
                    var window = _group.Items[i];
                    if (window is Views.MainWindow)
                    {
                        continue;
                    }

                    nodes.Add(new CombinedTreeNode(window, Owner, _templateProvider));
                }

                void GroupOnAdded(object? sender, TopLevel e)
                {
                    if (e is Views.MainWindow)
                    {
                        return;
                    }

                    nodes.Add(new CombinedTreeNode(e, Owner, _templateProvider));
                }

                void GroupOnRemoved(object? sender, TopLevel e)
                {
                    if (e is Views.MainWindow)
                    {
                        return;
                    }

                    var item = nodes.FirstOrDefault(node => ReferenceEquals(node.Visual, e));
                    if (item != null)
                    {
                        nodes.Remove(item);
                    }
                }

                _group.Added += GroupOnAdded;
                _group.Removed += GroupOnRemoved;

                _subscriptions.Add(new Disposable.AnonymousDisposable(() =>
                {
                    _group.Added -= GroupOnAdded;
                    _group.Removed -= GroupOnRemoved;
                }));
            }

            public override void Dispose()
            {
                _subscriptions.Dispose();
                base.Dispose();
            }
        }
    }
}
