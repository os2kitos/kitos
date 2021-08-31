using Core.ApplicationServices.Model.Shared;
using Infrastructure.Services.Types;
using Tests.Toolkit.Patterns;
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
    }
}
