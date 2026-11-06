using Core.DomainModel.ItSystemUsage.GDPR;
using Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Tests.Unit.Presentation.Web.Models.V2.Enums
{
    public class DpiaConductedMappingExtensionsTest : BaseEnumMapperTest<DpiaConductedChoice, DpiaConducted>
    {
        public override DpiaConducted ToDomainEnum(DpiaConductedChoice value)
        {
            return value.FromChoice();
        }

        public override DpiaConductedChoice ToChoice(DpiaConducted value)
        {
            return value.ToChoice();
        }
    }
}
