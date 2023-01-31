using Core.Abstractions.Types;
using Core.DomainModel.Tracking;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Controllers.API.V2.External.Deltas.Mapping
{
    public static class TrackedEntityTypeMappingExtensions
    {
        private static readonly EnumMap<TrackedEntityTypeChoice, TrackedEntityType> Mapping;

        static TrackedEntityTypeMappingExtensions()
        {
            Mapping = new EnumMap<TrackedEntityTypeChoice, TrackedEntityType>
            (
                (TrackedEntityTypeChoice.DataProcessingRegistration, TrackedEntityType.DataProcessingRegistration),
                (TrackedEntityTypeChoice.ItContract, TrackedEntityType.ItContract),
                (TrackedEntityTypeChoice.ItSystemUsage, TrackedEntityType.ItSystemUsage),
                (TrackedEntityTypeChoice.ItSystem, TrackedEntityType.ItSystem),
                (TrackedEntityTypeChoice.ItInterface, TrackedEntityType.ItInterface),
                (TrackedEntityTypeChoice.OrganizationUnit, TrackedEntityType.OrganizationUnit)
            );
        }

        public static TrackedEntityType ToDomainType(this TrackedEntityTypeChoice value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static TrackedEntityTypeChoice ToApiType(this TrackedEntityType value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}