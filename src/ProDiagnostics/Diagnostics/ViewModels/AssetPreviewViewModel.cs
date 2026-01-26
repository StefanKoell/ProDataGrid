using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Avalonia.Diagnostics.ViewModels
{
    internal sealed class AssetPreviewViewModel : ViewModelBase
    {
        private const int MaxTextPreviewBytes = 256 * 1024;
        private bool _isLoading;
        private bool _isLoaded;
        private string? _errorMessage;
        private string? _status;
        private Bitmap? _previewImage;
        private string? _previewText;
        private FontFamily? _previewFontFamily;
        private string? _fontFamilyName;
        private IReadOnlyList<AssetMetadataItem> _metadata = Array.Empty<AssetMetadataItem>();

        public AssetPreviewViewModel(AssetEntryViewModel asset)
        {
            Asset = asset;
            Title = asset.Name;
        }

        public AssetEntryViewModel Asset { get; }

        public string Title { get; }

        public string? Status
        {
            get => _status;
            private set => RaiseAndSetIfChanged(ref _status, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                if (RaiseAndSetIfChanged(ref _isLoading, value))
                {
                    RaisePropertyChanged(nameof(ShowPlaceholder));
                }
            }
        }

        public string? ErrorMessage
        {
            get => _errorMessage;
            private set
            {
                if (RaiseAndSetIfChanged(ref _errorMessage, value))
                {
                    RaisePropertyChanged(nameof(HasError));
                    RaisePropertyChanged(nameof(ShowPlaceholder));
                }
            }
        }

        public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

        public Bitmap? PreviewImage
        {
            get => _previewImage;
            private set
            {
                if (RaiseAndSetIfChanged(ref _previewImage, value))
                {
                    RaisePropertyChanged(nameof(IsImage));
                    RaisePropertyChanged(nameof(ShowPlaceholder));
                }
            }
        }

        public string? PreviewText
        {
            get => _previewText;
            private set
            {
                if (RaiseAndSetIfChanged(ref _previewText, value))
                {
                    RaisePropertyChanged(nameof(IsText));
                    RaisePropertyChanged(nameof(ShowPlaceholder));
                }
            }
        }

        public FontFamily? PreviewFontFamily
        {
            get => _previewFontFamily;
            private set
            {
                if (RaiseAndSetIfChanged(ref _previewFontFamily, value))
                {
                    RaisePropertyChanged(nameof(IsFont));
                    RaisePropertyChanged(nameof(ShowPlaceholder));
                }
            }
        }

        public string? FontFamilyName
        {
            get => _fontFamilyName;
            private set => RaiseAndSetIfChanged(ref _fontFamilyName, value);
        }

        public bool IsImage => PreviewImage != null;
        public bool IsText => PreviewText != null;
        public bool IsFont => PreviewFontFamily != null;
        public bool ShowPlaceholder => !IsLoading && !HasError && !IsImage && !IsText && !IsFont;

        public string SampleText => "The quick brown fox jumps over the lazy dog 0123456789";

        public IReadOnlyList<AssetMetadataItem> Metadata
        {
            get => _metadata;
            private set => RaiseAndSetIfChanged(ref _metadata, value);
        }

        public async Task LoadAsync()
        {
            if (_isLoaded)
            {
                return;
            }

            _isLoaded = true;
            IsLoading = true;
            Status = "Loading preview...";

            try
            {
                switch (Asset.Kind)
                {
                    case AssetKind.Image:
                        LoadImage();
                        break;
                    case AssetKind.Text:
                        await LoadTextAsync();
                        break;
                    case AssetKind.Font:
                        LoadFont();
                        break;
                    default:
                        ErrorMessage = "Preview is not available for this asset type.";
                        break;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                Status = null;
                IsLoading = false;
            }
        }

        private void LoadImage()
        {
            using var stream = AssetLoader.Open(Asset.Uri, null!);
            var bitmap = new Bitmap(stream);
            PreviewImage = bitmap;

            var metadata = CreateBaseMetadata();
            metadata.Add(new AssetMetadataItem("Pixel Size", $"{bitmap.PixelSize.Width} × {bitmap.PixelSize.Height}"));
            metadata.Add(new AssetMetadataItem("Dpi", $"{bitmap.Dpi.X:0.##} × {bitmap.Dpi.Y:0.##}"));
            metadata.Add(new AssetMetadataItem("Format", bitmap.Format?.ToString() ?? "Unknown"));
            metadata.Add(new AssetMetadataItem("Alpha", bitmap.AlphaFormat?.ToString() ?? "Unknown"));
            Metadata = metadata;
        }

        private async Task LoadTextAsync()
        {
            using var stream = AssetLoader.Open(Asset.Uri, null!);
            var result = await ReadTextPreviewAsync(stream);
            PreviewText = result.Text;

            var metadata = CreateBaseMetadata();
            metadata.Add(new AssetMetadataItem("Encoding", result.EncodingName));
            metadata.Add(new AssetMetadataItem("Bytes", result.BytesRead.ToString()));
            metadata.Add(new AssetMetadataItem("Characters", result.CharacterCount.ToString()));
            metadata.Add(new AssetMetadataItem("Truncated", result.Truncated ? "Yes" : "No"));
            Metadata = metadata;
        }

        private void LoadFont()
        {
            using var stream = AssetLoader.Open(Asset.Uri, null!);
            var familyName = TryReadFontFamilyName(stream) ?? Path.GetFileNameWithoutExtension(Asset.AssetPath);
            FontFamilyName = familyName;

            if (!string.IsNullOrWhiteSpace(familyName))
            {
                PreviewFontFamily = FontFamily.Parse($"{Asset.Uri}#{familyName}");
            }

            var metadata = CreateBaseMetadata();
            metadata.Add(new AssetMetadataItem("Family", familyName ?? "Unknown"));
            Metadata = metadata;
        }

        private List<AssetMetadataItem> CreateBaseMetadata()
        {
            return new List<AssetMetadataItem>
            {
                new AssetMetadataItem("Uri", Asset.UriText),
                new AssetMetadataItem("Assembly", Asset.AssemblyName),
                new AssetMetadataItem("Path", Asset.AssetPath),
                new AssetMetadataItem("Kind", Asset.KindDisplay)
            };
        }

        private static async Task<TextPreviewResult> ReadTextPreviewAsync(Stream stream)
        {
            var buffer = new byte[8192];
            var totalBytes = 0;
            var truncated = false;

            using var memory = new MemoryStream();

            while (totalBytes < MaxTextPreviewBytes)
            {
                var remaining = MaxTextPreviewBytes - totalBytes;
                var read = await stream.ReadAsync(buffer, 0, Math.Min(buffer.Length, remaining));
                if (read <= 0)
                {
                    break;
                }

                memory.Write(buffer, 0, read);
                totalBytes += read;
            }

            if (stream.ReadByte() != -1)
            {
                truncated = true;
            }

            memory.Position = 0;
            using var reader = new StreamReader(memory, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
            var text = await reader.ReadToEndAsync();
            var encodingName = reader.CurrentEncoding?.WebName ?? "unknown";
            return new TextPreviewResult(text, encodingName, totalBytes, text.Length, truncated);
        }

        private static string? TryReadFontFamilyName(Stream sourceStream)
        {
            Stream stream = sourceStream;
            if (!stream.CanSeek)
            {
                var copy = new MemoryStream();
                stream.CopyTo(copy);
                copy.Position = 0;
                stream = copy;
            }

            stream.Position = 0;
            using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

            if (!TrySeekNameTable(reader, out var nameTableOffset))
            {
                return null;
            }

            return ReadNameTable(reader, nameTableOffset);
        }

        private static bool TrySeekNameTable(BinaryReader reader, out long nameTableOffset)
        {
            nameTableOffset = 0;

            if (reader.BaseStream.Length < 12)
            {
                return false;
            }

            var tag = ReadUInt32BE(reader);
            if (tag == Tag("ttcf"))
            {
                ReadUInt32BE(reader); // version
                var fontCount = ReadUInt32BE(reader);
                if (fontCount == 0)
                {
                    return false;
                }

                var firstOffset = ReadUInt32BE(reader);
                reader.BaseStream.Position = firstOffset;
                tag = ReadUInt32BE(reader);
            }

            var numTables = ReadUInt16BE(reader);
            reader.BaseStream.Position += 6; // skip searchRange, entrySelector, rangeShift

            for (var i = 0; i < numTables; i++)
            {
                var tableTag = ReadUInt32BE(reader);
                ReadUInt32BE(reader); // checksum
                var offset = ReadUInt32BE(reader);
                ReadUInt32BE(reader); // length

                if (tableTag == Tag("name"))
                {
                    nameTableOffset = offset;
                    return true;
                }
            }

            return false;
        }

        private static string? ReadNameTable(BinaryReader reader, long nameTableOffset)
        {
            reader.BaseStream.Position = nameTableOffset;
            ReadUInt16BE(reader); // format
            var count = ReadUInt16BE(reader);
            var storageOffset = ReadUInt16BE(reader);

            var records = new List<NameRecord>();
            for (var i = 0; i < count; i++)
            {
                var platformId = ReadUInt16BE(reader);
                var encodingId = ReadUInt16BE(reader);
                var languageId = ReadUInt16BE(reader);
                var nameId = ReadUInt16BE(reader);
                var length = ReadUInt16BE(reader);
                var offset = ReadUInt16BE(reader);

                if (nameId == 1 || nameId == 16)
                {
                    records.Add(new NameRecord(platformId, encodingId, languageId, nameId, length, offset));
                }
            }

            if (records.Count == 0)
            {
                return null;
            }

            records.Sort((left, right) => ScoreRecord(right).CompareTo(ScoreRecord(left)));
            var best = records[0];

            var stringPos = nameTableOffset + storageOffset + best.Offset;
            reader.BaseStream.Position = stringPos;
            var bytes = reader.ReadBytes(best.Length);

            var encoding = best.PlatformId switch
            {
                0 => Encoding.BigEndianUnicode,
                3 => Encoding.BigEndianUnicode,
                _ => Encoding.ASCII
            };

            var text = encoding.GetString(bytes).TrimEnd('\0').Trim();
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }

        private static int ScoreRecord(NameRecord record)
        {
            var score = record.NameId == 16 ? 100 : 90;

            score += record.PlatformId switch
            {
                3 => 20,
                0 => 10,
                _ => 0
            };

            if (record.PlatformId == 3 && (record.EncodingId == 1 || record.EncodingId == 10))
            {
                score += 5;
            }

            return score;
        }

        private static ushort ReadUInt16BE(BinaryReader reader)
        {
            var b1 = reader.ReadByte();
            var b2 = reader.ReadByte();
            return (ushort)((b1 << 8) | b2);
        }

        private static uint ReadUInt32BE(BinaryReader reader)
        {
            var b1 = reader.ReadByte();
            var b2 = reader.ReadByte();
            var b3 = reader.ReadByte();
            var b4 = reader.ReadByte();
            return (uint)((b1 << 24) | (b2 << 16) | (b3 << 8) | b4);
        }

        private static uint Tag(string value)
        {
            return (uint)((value[0] << 24) | (value[1] << 16) | (value[2] << 8) | value[3]);
        }

        private readonly record struct NameRecord(
            ushort PlatformId,
            ushort EncodingId,
            ushort LanguageId,
            ushort NameId,
            ushort Length,
            ushort Offset);

        private readonly record struct TextPreviewResult(
            string Text,
            string EncodingName,
            int BytesRead,
            int CharacterCount,
            bool Truncated);
    }
}
