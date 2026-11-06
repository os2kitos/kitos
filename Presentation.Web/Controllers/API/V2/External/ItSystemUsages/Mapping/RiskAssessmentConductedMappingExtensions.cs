using Core.Abstractions.Types;
using Core.DomainModel.ItSystemUsage.GDPR;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public static class RiskAssessmentConductedMappingExtensions
    {
        private static readonly EnumMap<RiskAssessmentConductedChoice, RiskAssessmentConducted> Mapping;

        static RiskAssessmentConductedMappingExtensions()
        {
            Mapping = new EnumMap<RiskAssessmentConductedChoice, RiskAssessmentConducted>
            (
                (RiskAssessmentConductedChoice.No, RiskAssessmentConducted.No),
                (RiskAssessmentConductedChoice.Yes, RiskAssessmentConducted.Yes),
                (RiskAssessmentConductedChoice.DontKnow, RiskAssessmentConducted.DontKnow),
                (RiskAssessmentConductedChoice.Undecided, RiskAssessmentConducted.Undecided),
                (RiskAssessmentConductedChoice.Irrelevant, RiskAssessmentConducted.Irrelevant),
                (RiskAssessmentConductedChoice.NoMustBePrepared, RiskAssessmentConducted.NoMustBePrepared),
                (RiskAssessmentConductedChoice.NoMustNotBePrepared, RiskAssessmentConducted.NoMustNotBePrepared)
            );
        }

        public static RiskAssessmentConducted FromChoice(this RiskAssessmentConductedChoice value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static RiskAssessmentConductedChoice ToChoice(this RiskAssessmentConducted value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}
