using Presentation.Web.Controllers.API.V2.External.ItSystems.Mapping;
using Presentation.Web.Models.API.V2.Types.System;

namespace Tests.Unit.Presentation.Web.Models.V2.Enums
{
    public class SystemDeletionConflictMappingExtensionsTest : BaseEnumMapperTest<SystemDeletionConflict, Core.ApplicationServices.Model.System.SystemDeletionConflict>
    {
        public override Core.ApplicationServices.Model.System.SystemDeletionConflict ToDomainEnum(SystemDeletionConflict value)
        {
            return value.FromChoice();
        }

        public override SystemDeletionConflict ToChoice(Core.ApplicationServices.Model.System.SystemDeletionConflict value)
        {
            return value.ToChoice();
        }
    }
}
