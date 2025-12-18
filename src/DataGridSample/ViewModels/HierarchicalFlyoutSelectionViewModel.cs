// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Avalonia.Controls.DataGridHierarchical;
using Avalonia.Controls.Selection;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels;

public class HierarchicalFlyoutSelectionViewModel : ObservableObject
{
    private string _lastSelection = "None";

    public HierarchicalFlyoutSelectionViewModel()
    {
        FolderDestinationManager = new ItemFolderDestinationManagerViewModel(CreateTree());
        FolderDestinationManager.SelectionModel.SelectionChanged += OnSelectionChanged;
    }

    public ItemFolderDestinationManagerViewModel FolderDestinationManager { get; }

    public ObservableCollection<string> SelectionLog { get; } = [];

    public string LastSelection
    {
        get => _lastSelection;
        private set => SetProperty(ref _lastSelection, value);
    }

    private void OnSelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs e)
    {
        var added = (e.SelectedItems ?? Enumerable.Empty<object?>())
            .OfType<HierarchicalNode>()
            .Select(FormatNode)
            .ToArray();
        var removed = (e.DeselectedItems ?? Enumerable.Empty<object?>())
            .OfType<HierarchicalNode>()
            .Select(FormatNode)
            .ToArray();

        if (added.Length > 0)
        {
            LastSelection = string.Join(", ", added);
        }
        else if (FolderDestinationManager.SelectionModel.SelectedItems.Count > 0)
        {
            LastSelection = string.Join(
                ", ",
                FolderDestinationManager.SelectionModel.SelectedItems
                    .OfType<HierarchicalNode>()
                    .Select(FormatNode));
        }
        else
        {
            LastSelection = "None";
        }

        var addedText = added.Length == 0 ? "none" : string.Join(", ", added);
        var removedText = removed.Length == 0 ? "none" : string.Join(", ", removed);
        SelectionLog.Insert(0, $"Added: {addedText} | Removed: {removedText}");
    }

    private static string FormatNode(HierarchicalNode node)
    {
        return node.Item is ItemFolderDestinationViewModel folder
            ? folder.Name
            : "(unknown)";
    }

    private static ItemFolderDestinationViewModel CreateTree()
    {
        return new ItemFolderDestinationViewModel("Destinations", [
            new("Projects",
            [
                new("ProDataGrid",
                    [new("Docs"), new("Samples"), new("Playground")]),

                new("Avalonia"),
                new("Benchmarks")
            ]),

            new("Storage",
            [
                new("Images",
                    [new("Screenshots"), new("Assets")]),

                new("Archives",
                    [new("Backups"), new("Legacy exports")])
            ]),

            new("Team"),
            new("Scratchpad",
                [new("Ideas"), new("Temporary"), new("To triage")])
        ]);
    }
}

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
public class ItemFolderDestinationManagerViewModel
{
    public ItemFolderDestinationManagerViewModel(ItemFolderDestinationViewModel rootFolder)
    {
        SelectionModel = new SelectionModel<HierarchicalNode> { SingleSelect = true };

        var options = new HierarchicalOptions<ItemFolderDestinationViewModel>
        {
            AutoExpandRoot = true,
            ChildrenSelector = folder => folder.Folders,
            IsLeafSelector = folder => folder.Folders.Count == 0,
            MaxAutoExpandDepth = 3,
            VirtualizeChildren = true
        };

        HierarchicalModel = new HierarchicalModel<ItemFolderDestinationViewModel>(options);
        HierarchicalModel.SetRoot(rootFolder);

        Root = rootFolder;
    }

    public ItemFolderDestinationViewModel Root { get; }

    public HierarchicalModel<ItemFolderDestinationViewModel> HierarchicalModel { get; }

    public SelectionModel<HierarchicalNode> SelectionModel { get; }
}

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
public class ItemFolderDestinationViewModel
{
    public ItemFolderDestinationViewModel(string name, ObservableCollection<ItemFolderDestinationViewModel>? folders = null)
    {
        Name = name;
        Folders = folders ?? new ObservableCollection<ItemFolderDestinationViewModel>();
    }

    public string Name { get; }

    public ObservableCollection<ItemFolderDestinationViewModel> Folders { get; }
}
