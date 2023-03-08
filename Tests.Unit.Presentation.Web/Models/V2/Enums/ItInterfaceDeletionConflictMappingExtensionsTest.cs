using Presentation.Web.Controllers.API.V2.External.ItInterfaces.Mapping;
using Presentation.Web.Models.API.V2.Types.Interface;

namespace Tests.Unit.Presentation.Web.Models.V2.Enums
{
    public class ItInterfaceDeletionConflictMappingExtensionsTest : BaseEnumMapperTest<ItInterfaceDeletionConflict, Core.ApplicationServices.Model.Interface.ItInterfaceDeletionConflict>
    {
        public override Core.ApplicationServices.Model.Interface.ItInterfaceDeletionConflict ToDomainEnum(ItInterfaceDeletionConflict value)
        {
            return value.FromChoice();
        }

        public override ItInterfaceDeletionConflict ToChoice(Core.ApplicationServices.Model.Interface.ItInterfaceDeletionConflict value)
        {
            return value.ToChoice();
        }
    }
}
