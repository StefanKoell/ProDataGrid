using System;
using System.IO;

namespace Avalonia.Diagnostics.ViewModels
{
    internal sealed class AssetEntryViewModel : ViewModelBase
    {
        public AssetEntryViewModel(Uri uri, string assemblyName, string assetPath, AssetKind kind)
        {
            Uri = uri;
            UriText = uri.ToString();
            AssemblyName = assemblyName;
            AssetPath = assetPath;
            Name = Path.GetFileName(assetPath);
            Extension = Path.GetExtension(assetPath);
            Kind = kind;
            KindDisplay = kind.ToString();
            IsPreviewSupported = kind != AssetKind.Other;
        }

        public Uri Uri { get; }
        public string UriText { get; }
        public string AssemblyName { get; }
        public string AssetPath { get; }
        public string Name { get; }
        public string Extension { get; }
        public AssetKind Kind { get; }
        public string KindDisplay { get; }
        public bool IsPreviewSupported { get; }
    }
}
