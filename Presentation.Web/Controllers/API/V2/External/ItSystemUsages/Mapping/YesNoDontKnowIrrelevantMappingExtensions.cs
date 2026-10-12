using Core.Abstractions.Types;
using Core.DomainModel.ItSystemUsage.GDPR;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public static class YesNoDontKnowIrrelevantMappingExtensions
    {
        private static readonly EnumMap<YesNoDontKnowIrrelevantChoice, YesNoDontKnowIrrelevant> Mapping;

        static YesNoDontKnowIrrelevantMappingExtensions()
        {
            Mapping = new EnumMap<YesNoDontKnowIrrelevantChoice, YesNoDontKnowIrrelevant>
            (
                (YesNoDontKnowIrrelevantChoice.No, YesNoDontKnowIrrelevant.No),
                (YesNoDontKnowIrrelevantChoice.Yes, YesNoDontKnowIrrelevant.Yes),
                (YesNoDontKnowIrrelevantChoice.DontKnow, YesNoDontKnowIrrelevant.DontKnow),
                (YesNoDontKnowIrrelevantChoice.Irrelevant, YesNoDontKnowIrrelevant.Irrelevant),
                (YesNoDontKnowIrrelevantChoice.Undecided, YesNoDontKnowIrrelevant.Undecided)
            );
        }

        public static YesNoDontKnowIrrelevant FromChoice(this YesNoDontKnowIrrelevantChoice value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static YesNoDontKnowIrrelevantChoice ToChoice(this YesNoDontKnowIrrelevant value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}
