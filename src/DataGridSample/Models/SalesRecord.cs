using System;

namespace DataGridSample.Models
{
    public sealed class SalesRecord
    {
        public DateTime OrderDate { get; init; }

        public string Region { get; init; } = string.Empty;

        public string Segment { get; init; } = string.Empty;

        public string Category { get; init; } = string.Empty;

        public string Product { get; init; } = string.Empty;

        public double Sales { get; init; }

        public double Profit { get; init; }

        public int Quantity { get; init; }
    }
}
