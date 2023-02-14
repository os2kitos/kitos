using Core.Abstractions.Types;
using Core.DomainModel.ItSystem.DataTypes;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public static class DataOptionsMappingExtensions
    {
        private static readonly EnumMap<YesNoDontKnowChoice, DataOptions> Mapping;

        static DataOptionsMappingExtensions()
        {
            Mapping = new EnumMap<YesNoDontKnowChoice, DataOptions>
            (
                (YesNoDontKnowChoice.DontKnow, DataOptions.DONTKNOW),
                (YesNoDontKnowChoice.Yes, DataOptions.YES),
                (YesNoDontKnowChoice.No, DataOptions.NO),
                (YesNoDontKnowChoice.Undecided, DataOptions.UNDECIDED)
            );
        }

        public static DataOptions ToDataOptions(this YesNoDontKnowChoice value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static YesNoDontKnowChoice ToYesNoDontKnowChoice(this DataOptions value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}