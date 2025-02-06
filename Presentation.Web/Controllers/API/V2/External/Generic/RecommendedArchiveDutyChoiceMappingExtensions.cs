using Core.DomainModel;
using Core.Abstractions.Types;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Controllers.API.V2.External.Generic
{
    public static class RecommendedArchiveDutyChoiceMappingExtensions
    {
        private static readonly EnumMap<RecommendedArchiveDutyChoice, ArchiveDutyRecommendationTypes> Mapping;

        static RecommendedArchiveDutyChoiceMappingExtensions()
        {
            Mapping = new EnumMap<RecommendedArchiveDutyChoice, ArchiveDutyRecommendationTypes>
            (
                (RecommendedArchiveDutyChoice.B, ArchiveDutyRecommendationTypes.B),
                (RecommendedArchiveDutyChoice.K, ArchiveDutyRecommendationTypes.K),
                (RecommendedArchiveDutyChoice.NoRecommendation, ArchiveDutyRecommendationTypes.NoRecommendation),
                (RecommendedArchiveDutyChoice.Undecided, ArchiveDutyRecommendationTypes.Undecided),
                (RecommendedArchiveDutyChoice.PreserveDataCanDiscardDocuments, ArchiveDutyRecommendationTypes.PreserveDataCanDiscardDocuments)
            );
        }

        public static ArchiveDutyRecommendationTypes FromChoice(this RecommendedArchiveDutyChoice value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static RecommendedArchiveDutyChoice ToChoice(this ArchiveDutyRecommendationTypes value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}