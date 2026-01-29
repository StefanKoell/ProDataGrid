// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using System.Collections.Generic;

namespace ProCharts
{
    public enum ChartDataDeltaKind
    {
        None,
        Full,
        Reset,
        Insert,
        Remove,
        Replace,
        Move,
        Update
    }

    public sealed class ChartDataDelta
    {
        public static ChartDataDelta None { get; } = new ChartDataDelta(ChartDataDeltaKind.None);

        public static ChartDataDelta Full { get; } = new ChartDataDelta(ChartDataDeltaKind.Full);

        public ChartDataDelta(ChartDataDeltaKind kind, int index = 0, int oldCount = 0, int newCount = 0, IReadOnlyList<int>? seriesIndices = null)
        {
            Kind = kind;
            Index = index;
            OldCount = oldCount;
            NewCount = newCount;
            SeriesIndices = seriesIndices;
        }

        public ChartDataDeltaKind Kind { get; }

        public int Index { get; }

        public int OldCount { get; }

        public int NewCount { get; }

        public IReadOnlyList<int>? SeriesIndices { get; }

        public bool IsFullRefresh => Kind == ChartDataDeltaKind.Full || Kind == ChartDataDeltaKind.Reset;
    }

    public sealed class ChartDataUpdate
    {
        public ChartDataUpdate(ChartDataSnapshot snapshot, ChartDataDelta delta)
        {
            Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
            Delta = delta ?? throw new ArgumentNullException(nameof(delta));
        }

        public ChartDataSnapshot Snapshot { get; }

        public ChartDataDelta Delta { get; }
    }

    public sealed class ChartDataUpdateEventArgs : EventArgs
    {
        public ChartDataUpdateEventArgs(ChartDataUpdate update)
        {
            Update = update ?? throw new ArgumentNullException(nameof(update));
        }

        public ChartDataUpdate Update { get; }
    }
}
