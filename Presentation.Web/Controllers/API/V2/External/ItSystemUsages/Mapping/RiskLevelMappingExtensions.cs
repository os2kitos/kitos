using Core.Abstractions.Types;
using Core.DomainModel.ItSystem.DataTypes;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public static class RiskLevelMappingExtensions
    {
        private static readonly EnumMap<RiskLevelChoice, RiskLevel> Mapping;

        static RiskLevelMappingExtensions()
        {
            Mapping = new EnumMap<RiskLevelChoice, RiskLevel>
            (
                (RiskLevelChoice.High, RiskLevel.HIGH),
                (RiskLevelChoice.Medium, RiskLevel.MIDDLE),
                (RiskLevelChoice.Low, RiskLevel.LOW),
                (RiskLevelChoice.Undecided, RiskLevel.UNDECIDED)
            );
        }

        public static RiskLevel ToRiskLevel(this RiskLevelChoice value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static RiskLevelChoice ToRiskLevelChoice(this RiskLevel value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}