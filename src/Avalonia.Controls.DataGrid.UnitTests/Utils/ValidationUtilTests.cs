using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Controls.Utils;
using Avalonia.Data;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Utils
{
    public class ValidationUtilTests
    {
        [Fact]
        public void ContainsMemberName_Returns_True_For_Exact_Match()
        {
            var validationResult = new ValidationResult("error", new[] { "Name", "Age" });

            Assert.True(validationResult.ContainsMemberName("Age"));
        }

        [Fact]
        public void ContainsMemberName_Returns_False_When_Missing()
        {
            var validationResult = new ValidationResult("error", new[] { "Name", "Age" });

            Assert.False(validationResult.ContainsMemberName("Missing"));
        }

        [Fact]
        public void ContainsMemberName_Returns_True_For_Empty_Target_With_No_Members()
        {
            var validationResult = new ValidationResult("error");

            Assert.True(validationResult.ContainsMemberName(string.Empty));
        }

        [Fact]
        public void FindEqualValidationResult_Returns_Existing_When_Members_Match()
        {
            var existing = new ValidationResult("error", new[] { "Name", "Age" });
            var target = new ValidationResult("error", new[] { "Name", "Age" });
            var collection = new List<ValidationResult> { existing };

            var found = collection.FindEqualValidationResult(target);

            Assert.Same(existing, found);
        }

        [Fact]
        public void FindEqualValidationResult_Returns_Null_For_Different_Order()
        {
            var existing = new ValidationResult("error", new[] { "Name", "Age" });
            var target = new ValidationResult("error", new[] { "Age", "Name" });
            var collection = new List<ValidationResult> { existing };

            var found = collection.FindEqualValidationResult(target);

            Assert.Null(found);
        }

        [Fact]
        public void IsValid_Returns_True_For_Null_Or_Success()
        {
            Assert.True(ValidationUtil.IsValid(null));
            Assert.True(ValidationResult.Success.IsValid());
        }

        [Fact]
        public void IsValid_Returns_False_For_Failure()
        {
            Assert.False(new ValidationResult("error").IsValid());
        }

        [Fact]
        public void UnpackException_Filters_BindingChainExceptions()
        {
            var bindingError = new BindingChainException("binding");
            var otherError = new InvalidOperationException("other");

            var exceptions = ValidationUtil.UnpackException(new AggregateException(bindingError, otherError)).ToList();

            Assert.Single(exceptions);
            Assert.Same(otherError, exceptions[0]);
        }

        [Fact]
        public void UnpackException_Returns_Empty_For_Null()
        {
            var exceptions = ValidationUtil.UnpackException(null);

            Assert.Empty(exceptions);
        }

        [Fact]
        public void UnpackException_Returns_Empty_When_Only_BindingChain()
        {
            var exceptions = ValidationUtil.UnpackException(new BindingChainException("binding"));

            Assert.Empty(exceptions);
        }

        [Fact]
        public void UnpackException_Returns_Single_For_NonAggregate()
        {
            var error = new InvalidOperationException("other");

            var exceptions = ValidationUtil.UnpackException(error).ToList();

            Assert.Single(exceptions);
            Assert.Same(error, exceptions[0]);
        }

        [Fact]
        public void UnpackDataValidationException_Returns_ErrorData()
        {
            var errorData = new object();
            var exception = new DataValidationException(errorData);

            var result = ValidationUtil.UnpackDataValidationException(exception);

            Assert.Same(errorData, result);
        }

        [Fact]
        public void UnpackDataValidationException_Returns_Exception_For_NonDataValidation()
        {
            var exception = new InvalidOperationException("other");

            var result = ValidationUtil.UnpackDataValidationException(exception);

            Assert.Same(exception, result);
        }

        [Fact]
        public void GetValidationSeverity_Uses_DataGridValidationResult_Severity()
        {
            var warning = new DataValidationException(new DataGridValidationResult("warning", DataGridValidationSeverity.Warning));

            var result = ValidationUtil.GetValidationSeverity(warning);

            Assert.Equal(DataGridValidationSeverity.Warning, result);
        }

        [Fact]
        public void GetValidationSeverity_Uses_Strongest_Severity_From_Enumerable()
        {
            var results = new[]
            {
                new DataGridValidationResult("info", DataGridValidationSeverity.Info),
                new DataGridValidationResult("warning", DataGridValidationSeverity.Warning)
            };
            var exception = new DataValidationException(results);

            var result = ValidationUtil.GetValidationSeverity(exception);

            Assert.Equal(DataGridValidationSeverity.Warning, result);
        }

        [Fact]
        public void GetValidationSeverity_Defaults_To_Error_For_String_Data()
        {
            var exception = new DataValidationException("error");

            var result = ValidationUtil.GetValidationSeverity(exception);

            Assert.Equal(DataGridValidationSeverity.Error, result);
        }

        [Fact]
        public void ContainsEqualValidationResult_Uses_Equality_Check()
        {
            var existing = new ValidationResult("error", new[] { "Name", "Age" });
            var target = new ValidationResult("error", new[] { "Name", "Age" });
            var collection = new List<ValidationResult> { existing };

            Assert.True(collection.ContainsEqualValidationResult(target));
        }

        [Fact]
        public void AddIfNew_Adds_Only_Once()
        {
            var existing = new ValidationResult("error", new[] { "Name" });
            var duplicate = new ValidationResult("error", new[] { "Name" });
            var unique = new ValidationResult("error", new[] { "Age" });
            var collection = new List<ValidationResult> { existing };

            collection.AddIfNew(duplicate);
            collection.AddIfNew(unique);

            Assert.Equal(2, collection.Count);
            Assert.Contains(existing, collection);
            Assert.Contains(unique, collection);
        }

        [Fact]
        public void AddExceptionIfNew_Uses_Message_To_Check_Duplicates()
        {
            var duplicate = new InvalidOperationException("error");
            var unique = new InvalidOperationException("different");
            var collection = new List<Exception> { new InvalidOperationException("error") };

            collection.AddExceptionIfNew(duplicate);
            collection.AddExceptionIfNew(unique);

            Assert.Equal(2, collection.Count);
            Assert.Contains(collection, exception => exception.Message == "error");
            Assert.Contains(collection, exception => exception.Message == "different");
        }

        [Fact]
        public void CatchNonCriticalExceptions_Swallows_NonCritical()
        {
            var invoked = false;

            ValidationUtil.CatchNonCriticalExceptions(() =>
            {
                invoked = true;
                throw new InvalidOperationException("noncritical");
            });

            Assert.True(invoked);
        }

        [Fact]
        public void CatchNonCriticalExceptions_Rethrows_Critical()
        {
            Assert.Throws<OutOfMemoryException>(
                () => ValidationUtil.CatchNonCriticalExceptions(() => throw new OutOfMemoryException()));
        }

        [Fact]
        public void IsCriticalException_Identifies_Critical_Types()
        {
            var threadAbortException =
                (ThreadAbortException)Activator.CreateInstance(typeof(ThreadAbortException), true)!;

            Assert.True(ValidationUtil.IsCriticalException(new OutOfMemoryException()));
            Assert.True(ValidationUtil.IsCriticalException(new StackOverflowException()));
            Assert.True(ValidationUtil.IsCriticalException(new AccessViolationException()));
            Assert.True(ValidationUtil.IsCriticalException(threadAbortException));
            Assert.False(ValidationUtil.IsCriticalException(new InvalidOperationException()));
        }
    }
}
