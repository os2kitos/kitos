using Core.DomainModel;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Tests.Unit.Presentation.Web.Models.V2.Enums
{
    public class RecommendedArchiveDutyChoiceMappingTest : BaseEnumMapperTest<RecommendedArchiveDutyChoice, ArchiveDutyRecommendationTypes>
    {
        public override ArchiveDutyRecommendationTypes ToDomainEnum(RecommendedArchiveDutyChoice value)
        {
            return value.FromChoice();
        }

        public override RecommendedArchiveDutyChoice ToChoice(ArchiveDutyRecommendationTypes value)
        {
            return value.ToChoice();
        }
    }
}
