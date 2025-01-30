using Core.Abstractions.Types;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public static class ArchiveDutyMappingExtensions
    {
        private static readonly EnumMap<ArchiveDutyChoice, ArchiveDutyTypes> Mapping;

        static ArchiveDutyMappingExtensions()
        {
            Mapping = new EnumMap<ArchiveDutyChoice, ArchiveDutyTypes>
            (
                (ArchiveDutyChoice.B, ArchiveDutyTypes.B),
                (ArchiveDutyChoice.K, ArchiveDutyTypes.K),
                (ArchiveDutyChoice.Undecided, ArchiveDutyTypes.Undecided),
                (ArchiveDutyChoice.Unknown, ArchiveDutyTypes.Unknown),
                (ArchiveDutyChoice.PreserveDataCanDiscardDocuments, ArchiveDutyTypes.PreserveDataCanDiscardDocuments)
            );
        }

        public static ArchiveDutyTypes ToArchiveDutyTypes(this ArchiveDutyChoice value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static ArchiveDutyChoice ToArchiveDutyChoice(this ArchiveDutyTypes value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}