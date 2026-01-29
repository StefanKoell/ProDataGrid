// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using Avalonia.Input;

namespace ProCharts.Avalonia
{
    public enum ChartClipboardFormat
    {
        Png,
        Svg
    }

    public static class ChartClipboardFormats
    {
        public static DataFormat<string> Svg { get; } =
            DataFormat.CreateStringPlatformFormat("image/svg+xml");
    }
}
