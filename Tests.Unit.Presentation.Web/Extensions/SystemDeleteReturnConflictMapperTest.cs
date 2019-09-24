using Core.ApplicationServices.Model.System;
using Presentation.Web.Extensions;
using Presentation.Web.Models.ItSystem;
using Xunit;

namespace Tests.Unit.Presentation.Web.Extensions
{
    public class SystemDeleteReturnConflictMapperTest
    {
        [Theory]
        [InlineData(DeleteResult.InUse, SystemDeleteConflict.InUse)]
        [InlineData(DeleteResult.HasChildren, SystemDeleteConflict.HasChildren)]
        [InlineData(DeleteResult.HasInterfaceExhibits, SystemDeleteConflict.HasInterfaceExhibits)]
        public void CanMapToConflict(DeleteResult deleteResult, SystemDeleteConflict conflict)
        {
            Assert.Equal(conflict, deleteResult.MapToConflict());
        }
    }
}
