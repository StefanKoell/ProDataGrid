// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.DataGridSorting;
using DataGridSample.Models;
using DataGridSample.Helpers;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class SortingModelViewModel : ObservableObject
    {
        private bool _ownsSortDescriptions = true;
        private bool _multiSortEnabled = true;
        private SortCycleMode _sortCycleMode = SortCycleMode.AscendingDescending;
        private readonly DataGridColumnDefinition _nameColumn;
        private readonly DataGridColumnDefinition _regionColumn;
        private readonly DataGridColumnDefinition _populationColumn;
        private readonly DataGridColumnDefinition _areaColumn;

        public SortingModelViewModel()
        {
            Items = new ObservableCollection<Country>(Countries.All);
            ItemsView = new DataGridCollectionView(Items);

            ApplySortCommand = new RelayCommand(_ => ApplyProgrammaticSort());
            ExternalSortCommand = new RelayCommand(_ => PushExternalSorts());
            ClearSortsCommand = new RelayCommand(_ => ItemsView.SortDescriptions.Clear());

            _nameColumn = new DataGridTextColumnDefinition
            {
                Header = "Name",
                Binding = ColumnDefinitionBindingFactory.CreateBinding<Country, string>(nameof(Country.Name), c => c.Name),
                SortMemberPath = nameof(Country.Name),
                Width = new DataGridLength(1.2, DataGridLengthUnitType.Star)
            };
            _regionColumn = new DataGridTextColumnDefinition
            {
                Header = "Region",
                Binding = ColumnDefinitionBindingFactory.CreateBinding<Country, string>(nameof(Country.Region), c => c.Region),
                SortMemberPath = nameof(Country.Region),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            };
            _populationColumn = new DataGridNumericColumnDefinition
            {
                Header = "Population",
                Binding = ColumnDefinitionBindingFactory.CreateBinding<Country, int>(nameof(Country.Population), c => c.Population),
                SortMemberPath = nameof(Country.Population),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                FormatString = "N0"
            };
            _areaColumn = new DataGridNumericColumnDefinition
            {
                Header = "Area",
                Binding = ColumnDefinitionBindingFactory.CreateBinding<Country, int>(nameof(Country.Area), c => c.Area),
                SortMemberPath = nameof(Country.Area),
                Width = new DataGridLength(0.9, DataGridLengthUnitType.Star),
                FormatString = "N0"
            };

            ColumnDefinitions = new ObservableCollection<DataGridColumnDefinition>
            {
                _nameColumn,
                _regionColumn,
                _populationColumn,
                _areaColumn
            };
        }

        public ObservableCollection<Country> Items { get; }

        public DataGridCollectionView ItemsView { get; }

        public ObservableCollection<DataGridColumnDefinition> ColumnDefinitions { get; }

        public bool OwnsSortDescriptions
        {
            get => _ownsSortDescriptions;
            set => SetProperty(ref _ownsSortDescriptions, value);
        }

        public bool MultiSortEnabled
        {
            get => _multiSortEnabled;
            set => SetProperty(ref _multiSortEnabled, value);
        }

        public SortCycleMode SortCycleMode
        {
            get => _sortCycleMode;
            set => SetProperty(ref _sortCycleMode, value);
        }

        public RelayCommand ApplySortCommand { get; }

        public RelayCommand ExternalSortCommand { get; }

        public RelayCommand ClearSortsCommand { get; }

        private void ApplyProgrammaticSort()
        {
            using (ItemsView.DeferRefresh())
            {
                ItemsView.SortDescriptions.Clear();
                ItemsView.SortDescriptions.Add(CreateSortDescription(_nameColumn, System.ComponentModel.ListSortDirection.Ascending));
                ItemsView.SortDescriptions.Add(CreateSortDescription(_populationColumn, System.ComponentModel.ListSortDirection.Descending));
            }
        }

        private void PushExternalSorts()
        {
            using (ItemsView.DeferRefresh())
            {
                ItemsView.SortDescriptions.Clear();
                ItemsView.SortDescriptions.Add(CreateSortDescription(_regionColumn, System.ComponentModel.ListSortDirection.Descending));
                // Intentional duplicate to showcase deduplication when syncing from the view.
                ItemsView.SortDescriptions.Add(CreateSortDescription(_regionColumn, System.ComponentModel.ListSortDirection.Ascending));
            }
        }

        private DataGridSortDescription CreateSortDescription(DataGridColumnDefinition definition, System.ComponentModel.ListSortDirection direction)
        {
            if (definition?.ValueAccessor != null)
            {
                return DataGridSortDescription.FromAccessor(
                    definition.ValueAccessor,
                    direction,
                    ItemsView.Culture,
                    definition.SortMemberPath);
            }

            var fallbackPath = definition?.SortMemberPath;
            return !string.IsNullOrEmpty(fallbackPath)
                ? DataGridSortDescription.FromPath(fallbackPath, direction, ItemsView.Culture)
                : DataGridSortDescription.FromComparer(Comparer<object>.Default, direction);
        }
    }
}
