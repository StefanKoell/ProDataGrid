
using Avalonia.Controls.Utils;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Utils
{
    public class ReflectionHelperTests
    {
        [Fact]
        public void SplitPropertyPath_Splits_PropertyPath_With_Cast()
        {
            var path = "(Type).Property";
            var expected = new [] { "Property" };

            var result = TypeHelper.SplitPropertyPath(path);
            
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetPropertyOrIndexer_Prefers_Int_Indexer()
        {
            var type = typeof(IndexerType);

            var property = type.GetPropertyOrIndexer("[4]", out var index);

            Assert.NotNull(property);
            Assert.Equal("Item", property!.Name);
            Assert.Equal(new object[] { 4 }, index);
        }

        [Fact]
        public void GetNestedProperty_Resolves_Indexer_Path()
        {
            var model = new ParentWithChildren
            {
                Children = new[]
                {
                    new Child { Name = "first" },
                    new Child { Name = "second" }
                }
            };

            object? item = model;
            var property = typeof(ParentWithChildren).GetNestedProperty("Children[1].Name", ref item);

            Assert.NotNull(property);
            Assert.Equal("Name", property!.Name);
            Assert.Equal("second", item);
        }

        [Fact]
        public void GetDisplayName_Uses_DisplayAttribute_ShortName()
        {
            var shortName = typeof(DisplayNameParent).GetDisplayName("Child.Label");

            Assert.Equal("ShortLabel", shortName);
        }

        private class IndexerType
        {
            public string this[int index] => $"int-{index}";

            public string this[string key] => $"string-{key}";
        }

        private class ParentWithChildren
        {
            public IReadOnlyList<Child>? Children { get; set; }
        }

        private class Child
        {
            public string? Name { get; set; }
        }

        private class DisplayNameParent
        {
            public DisplayNameChild? Child { get; set; }
        }

        private class DisplayNameChild
        {
            [Display(Name = "LongName", ShortName = "ShortLabel")]
            public string? Label { get; set; }
        }
    }
}
