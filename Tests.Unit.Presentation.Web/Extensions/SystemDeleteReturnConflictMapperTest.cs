using System;
using Core.ApplicationServices.Model.System;
using Presentation.Web.Extensions;
using Presentation.Web.Models.ItSystem;
using Xunit;

namespace Tests.Unit.Presentation.Web.Extensions
{
    public class SystemDeleteReturnConflictMapperTest
    {
        [Theory]
        [InlineData(SystemDeleteResult.InUse, SystemDeleteConflict.InUse)]
        [InlineData(SystemDeleteResult.HasChildren, SystemDeleteConflict.HasChildren)]
        [InlineData(SystemDeleteResult.HasInterfaceExhibits, SystemDeleteConflict.HasInterfaceExhibits)]
        public void Can_Map_To_Conflict(SystemDeleteResult deleteResult, SystemDeleteConflict conflict)
        {
            Assert.Equal(conflict, deleteResult.MapToConflict());
        }

        [Theory]
        [InlineData(SystemDeleteResult.Forbidden)]
        [InlineData(SystemDeleteResult.NotFound)]
        [InlineData(SystemDeleteResult.UnknownError)]
        [InlineData(SystemDeleteResult.Ok)]
        public void Can_Not_Map_To_conflict(SystemDeleteResult deleteResult)
        {
            Assert.Throws<NotSupportedException>(() => deleteResult.MapToConflict());
        }
    }
}
