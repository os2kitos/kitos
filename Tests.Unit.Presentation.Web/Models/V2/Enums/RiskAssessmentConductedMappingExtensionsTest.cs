using Core.DomainModel.ItSystemUsage.GDPR;
using Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Tests.Unit.Presentation.Web.Models.V2.Enums
{
    public class RiskAssessmentConductedMappingExtensionsTest : BaseEnumMapperTest<RiskAssessmentConductedChoice, RiskAssessmentConducted>
    {
        public override RiskAssessmentConducted ToDomainEnum(RiskAssessmentConductedChoice value)
        {
            return value.FromChoice();
        }

        public override RiskAssessmentConductedChoice ToChoice(RiskAssessmentConducted value)
        {
            return value.ToChoice();
        }
    }
}
