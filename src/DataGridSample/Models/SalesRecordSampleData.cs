using System;
using System.Collections.Generic;

namespace DataGridSample.Models
{
    public static class SalesRecordSampleData
    {
        private static readonly string[] s_regions = { "North", "South", "East", "West" };
        private static readonly string[] s_segments = { "Consumer", "Corporate", "Home Office" };
        private static readonly string[] s_categories = { "Furniture", "Office Supplies", "Technology" };
        private static readonly Dictionary<string, string[]> s_productsByCategory = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Furniture"] = new[] { "Chair", "Desk", "Bookcase", "Table", "Sofa" },
            ["Office Supplies"] = new[] { "Paper", "Binders", "Labels", "Pens", "Storage" },
            ["Technology"] = new[] { "Laptop", "Monitor", "Phone", "Printer", "Accessories" }
        };

        public static IReadOnlyList<string> Regions => s_regions;

        public static IReadOnlyList<string> Segments => s_segments;

        public static IReadOnlyList<string> Categories => s_categories;

        public static IEnumerable<SalesRecord> CreateSalesRecords(int count, int seed = 1729)
        {
            var random = new Random(seed);
            var start = new DateTime(DateTime.Today.Year - 2, 1, 1);
            var days = Math.Max(1, (DateTime.Today - start).Days);

            for (var i = 0; i < count; i++)
            {
                var region = s_regions[random.Next(s_regions.Length)];
                var segment = s_segments[random.Next(s_segments.Length)];
                var category = s_categories[random.Next(s_categories.Length)];
                var product = s_productsByCategory[category][random.Next(s_productsByCategory[category].Length)];
                var orderDate = start.AddDays(random.Next(days));

                var quantity = random.Next(1, 12);
                var unitPrice = random.NextDouble() * 900 + 25;
                var sales = Math.Round(unitPrice * quantity, 2);
                var margin = random.NextDouble() * 0.35 - 0.05;
                var profit = Math.Round(sales * margin, 2);

                yield return new SalesRecord
                {
                    OrderDate = orderDate,
                    Region = region,
                    Segment = segment,
                    Category = category,
                    Product = product,
                    Sales = sales,
                    Profit = profit,
                    Quantity = quantity
                };
            }
        }
    }
}
