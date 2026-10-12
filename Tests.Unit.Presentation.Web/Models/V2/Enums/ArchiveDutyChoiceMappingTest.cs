using Core.DomainModel;
using Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Tests.Unit.Presentation.Web.Models.V2.Enums
{
    public class ArchiveDutyChoiceMappingTest : BaseEnumMapperTest<ArchiveDutyChoice, ArchiveDutyTypes>
    {
        public override ArchiveDutyTypes ToDomainEnum(ArchiveDutyChoice value)
        {
            return value.ToArchiveDutyTypes();
        }

        public override ArchiveDutyChoice ToChoice(ArchiveDutyTypes value)
        {
            return value.ToArchiveDutyChoice();
        }
    }
}
