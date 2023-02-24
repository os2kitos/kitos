using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

using Tests.Toolkit.Patterns;
using Tests.Toolkit.TestInputs;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    /// <summary>
    /// Base class for write model mapping. Supports the different general assertions related to write models
    /// </summary>
    public abstract class WriteModelMapperTestBase : WithAutoFixture
    {
        protected static T AssertPropertyContainsDataChange<T>(OptionalValueChange<Maybe<T>> sourceData)
        {
            Assert.True(sourceData.HasChange);
            Assert.True(sourceData.NewValue.HasValue);
            return sourceData.NewValue.Value;
        }

        protected static T AssertPropertyContainsDataChange<T>(Maybe<T> sourceData)
        {
            Assert.True(sourceData.HasValue);
            return sourceData.Value;
        }

        protected static T AssertPropertyContainsDataChange<T>(OptionalValueChange<T> sourceData)
        {
            Assert.True(sourceData.HasChange);
            return sourceData.NewValue;
        }

        protected static void AssertPropertyContainsResetDataChange<T>(OptionalValueChange<Maybe<T>> sourceData)
        {
            Assert.True(sourceData.HasChange);
            Assert.True(sourceData.NewValue.IsNone);
        }

        protected static IEnumerable<object[]> CreateGetUndefinedSectionsInput(int numberOfInputParameters)
        {
            return BooleanInputMatrixFactory.Create(numberOfInputParameters);
        }

        protected static HashSet<string> GetAllInputPropertyNames<T>()
        {
            return typeof(T).GetProperties().Select(x => x.Name).ToHashSet();
        }
    }
}
