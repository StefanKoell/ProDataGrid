using System;
using Avalonia.Controls;
using Avalonia.Diagnostics.ViewModels;
using Avalonia.Markup.Xaml;

namespace Avalonia.Diagnostics.Views
{
    partial class AssetPreviewWindow : Window
    {
        public AssetPreviewWindow()
        {
            InitializeComponent();
            Opened += OnOpened;
        }

        private async void OnOpened(object? sender, EventArgs e)
        {
            if (DataContext is AssetPreviewViewModel viewModel)
            {
                await viewModel.LoadAsync();
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
