using Core.Abstractions.Types;
using Core.DomainModel.ItSystemUsage.GDPR;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public static class DpiaConductedMappingExtensions
    {
        private static readonly EnumMap<DpiaConductedChoice, DpiaConducted> Mapping;

        static DpiaConductedMappingExtensions()
        {
            Mapping = new EnumMap<DpiaConductedChoice, DpiaConducted>
            (
                (DpiaConductedChoice.No, DpiaConducted.No),
                (DpiaConductedChoice.Yes, DpiaConducted.Yes),
                (DpiaConductedChoice.DontKnow, DpiaConducted.DontKnow),
                (DpiaConductedChoice.Undecided, DpiaConducted.Undecided),
                (DpiaConductedChoice.NoMustBePrepared, DpiaConducted.NoMustBePrepared),
                (DpiaConductedChoice.NoMustNotBePrepared, DpiaConducted.NoMustNotBePrepared)
            );
        }

        public static DpiaConducted FromChoice(this DpiaConductedChoice value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static DpiaConductedChoice ToChoice(this DpiaConducted value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}
