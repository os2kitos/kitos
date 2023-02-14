using Core.Abstractions.Types;
using Core.DomainModel.Shared;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Controllers.API.V2.External.Generic
{
    public static class YesNoIrrelevantMappingExtensions
    {
        private static readonly EnumMap<YesNoIrrelevantChoice, YesNoIrrelevantOption> Mapping;

        static YesNoIrrelevantMappingExtensions()
        {
            Mapping = new EnumMap<YesNoIrrelevantChoice, YesNoIrrelevantOption>
            (
                (YesNoIrrelevantChoice.Yes, YesNoIrrelevantOption.YES),
                (YesNoIrrelevantChoice.No, YesNoIrrelevantOption.NO),
                (YesNoIrrelevantChoice.Irrelevant, YesNoIrrelevantOption.IRRELEVANT),
                (YesNoIrrelevantChoice.Undecided, YesNoIrrelevantOption.UNDECIDED)
            );
        }

        public static YesNoIrrelevantOption ToYesNoIrrelevantOption(this YesNoIrrelevantChoice value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static YesNoIrrelevantChoice ToYesNoIrrelevantChoice(this YesNoIrrelevantOption value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}