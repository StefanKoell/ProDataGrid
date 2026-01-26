namespace Avalonia.Diagnostics.ViewModels
{
    internal sealed class AssetMetadataItem
    {
        public AssetMetadataItem(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }
        public string Value { get; }
    }
}
