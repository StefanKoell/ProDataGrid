using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Controls;
using System.Linq;
using Xunit;

namespace Avalonia.Controls.DataGridTests
{
    public class DataGridItemPropertyDescriptorTests
    {
        [Fact]
        public void CreateDescriptors_Uses_ITypedList()
        {
            var list = new DescriptorList();

            var descriptors = DataGridItemPropertyDescriptor.CreateDescriptors(list, typeof(PlainPoco));

            var descriptor = Assert.Single(descriptors!);
            Assert.Equal("FromTypedList", descriptor.Name);
            Assert.Null(descriptor.PropertyInfo);
            Assert.IsType<CustomPropertyDescriptor>(descriptor.PropertyDescriptor);
        }

        [Fact]
        public void CreateDescriptors_Uses_Reflection_For_Poco()
        {
            var descriptors = DataGridItemPropertyDescriptor.CreateDescriptors(items: null, dataType: typeof(PlainPoco));

            Assert.Equal(2, descriptors!.Length);
            Assert.All(descriptors, d =>
            {
                Assert.NotNull(d.PropertyInfo);
                Assert.Null(d.PropertyDescriptor);
            });
        }

        [Fact]
        public void CreateDescriptors_Uses_Custom_Provider()
        {
            TypeDescriptor.Refresh(typeof(ProvidedType));
            TypeDescriptor.AddProvider(new ProvidedTypeProvider(), typeof(ProvidedType));
            var descriptors = DataGridItemPropertyDescriptor.CreateDescriptors(new[] { new ProvidedType() }, dataType: null);

            var descriptor = Assert.Single(descriptors!);
            Assert.Equal("FromProvider", descriptor.Name);
            Assert.Null(descriptor.PropertyInfo);
            Assert.IsType<CustomPropertyDescriptor>(descriptor.PropertyDescriptor);
        }

        [Fact]
        public void CreateDescriptors_Uses_CustomTypeDescriptor()
        {
            var descriptors = DataGridItemPropertyDescriptor.CreateDescriptors(new[] { new CustomDescriptorType() }, dataType: null);

            var descriptor = Assert.Single(descriptors!);
            Assert.Equal("FromICustomTypeDescriptor", descriptor.Name);
            Assert.Null(descriptor.PropertyInfo);
            Assert.IsType<CustomPropertyDescriptor>(descriptor.PropertyDescriptor);
        }

        [Fact]
        public void CreateDescriptors_Falls_Back_To_Item_TypeDescriptor_When_DataType_Null()
        {
            var descriptors = DataGridItemPropertyDescriptor.CreateDescriptors(new[] { new PlainPoco() }, dataType: null);

            Assert.Equal(2, descriptors!.Length);
            Assert.All(descriptors, d =>
            {
                Assert.Null(d.PropertyInfo);
                Assert.NotNull(d.PropertyDescriptor);
            });
        }

        private class PlainPoco
        {
            public int A { get; set; }
            public string? B { get; set; }
        }

        private class DescriptorList : List<PlainPoco>, ITypedList
        {
            private readonly PropertyDescriptorCollection _properties = new(new[]
            {
                new CustomPropertyDescriptor("FromTypedList", typeof(PlainPoco), typeof(string))
            });

            public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[]? listAccessors) => _properties;

            public string GetListName(PropertyDescriptor[]? listAccessors) => nameof(DescriptorList);
        }

        [TypeDescriptionProvider(typeof(ProvidedTypeProvider))]
        public class ProvidedType
        {
            public int Ignored { get; set; }
        }

        public class ProvidedTypeProvider : TypeDescriptionProvider
        {
            private readonly ICustomTypeDescriptor _descriptor;

            public ProvidedTypeProvider() : base(TypeDescriptor.GetProvider(typeof(object)))
            {
                _descriptor = new SimpleTypeDescriptor(new PropertyDescriptorCollection(new PropertyDescriptor[]
                {
                    new CustomPropertyDescriptor("FromProvider", typeof(ProvidedType), typeof(string))
                }));
            }

            public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object? instance) => _descriptor;
        }

        private class CustomDescriptorType : ICustomTypeDescriptor
        {
            private readonly PropertyDescriptorCollection _properties = new(new[]
            {
                new CustomPropertyDescriptor("FromICustomTypeDescriptor", typeof(CustomDescriptorType), typeof(int))
            });

            public AttributeCollection GetAttributes() => AttributeCollection.Empty;

            public string? GetClassName() => nameof(CustomDescriptorType);

            public string? GetComponentName() => nameof(CustomDescriptorType);

            public TypeConverter? GetConverter() => null;

            public EventDescriptor? GetDefaultEvent() => null;

            public PropertyDescriptor? GetDefaultProperty() => null;

            public object? GetEditor(Type editorBaseType) => null;

            public EventDescriptorCollection GetEvents(Attribute[]? attributes) => EventDescriptorCollection.Empty;

            public EventDescriptorCollection GetEvents() => EventDescriptorCollection.Empty;

            public PropertyDescriptorCollection GetProperties(Attribute[]? attributes) => _properties;

            public PropertyDescriptorCollection GetProperties() => _properties;

            public object? GetPropertyOwner(PropertyDescriptor? pd) => this;
        }

        private class CustomPropertyDescriptor : PropertyDescriptor
        {
            private readonly Type _componentType;
            private readonly Type _propertyType;
            private object? _value;

            public CustomPropertyDescriptor(string name, Type componentType, Type propertyType)
                : base(name, null)
            {
                _componentType = componentType;
                _propertyType = propertyType;
            }

            public override Type ComponentType => _componentType;

            public override bool IsReadOnly => false;

            public override Type PropertyType => _propertyType;

            public override bool CanResetValue(object component) => false;

            public override object? GetValue(object? component) => _value;

            public override void ResetValue(object component) { }

            public override void SetValue(object? component, object? value) => _value = value;

            public override bool ShouldSerializeValue(object component) => false;
        }

        private class SimpleTypeDescriptor : CustomTypeDescriptor
        {
            private readonly PropertyDescriptorCollection _properties;

            public SimpleTypeDescriptor(PropertyDescriptorCollection properties)
            {
                _properties = properties;
            }

            public override PropertyDescriptorCollection GetProperties() => _properties;

            public override PropertyDescriptorCollection GetProperties(Attribute[]? attributes) => _properties;
        }
    }
}
