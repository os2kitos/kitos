using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Tests.Unit.Presentation.Web.Models.V2.Enums
{
    public class LifeCycleStatusMappingTest : BaseEnumMapperTest<LifeCycleStatusChoice, LifeCycleStatusType>
    {
        public override LifeCycleStatusType ToDomainEnum(LifeCycleStatusChoice value)
        {
            return value.ToLifeCycleStatusType();
        }

        public override LifeCycleStatusChoice ToChoice(LifeCycleStatusType value)
        {
            return value.ToLifeCycleStatusChoice();
        }
    }
}
