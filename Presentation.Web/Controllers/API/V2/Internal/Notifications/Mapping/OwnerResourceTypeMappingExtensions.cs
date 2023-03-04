using Core.Abstractions.Types;
using Core.DomainModel.Shared;
using Presentation.Web.Models.API.V2.Types.Notifications;

namespace Presentation.Web.Controllers.API.V2.Internal.Notifications.Mapping
{
    public static class OwnerResourceTypeMappingExtensions
    {
        private static readonly EnumMap<OwnerResourceType, RelatedEntityType> Mapping;
        static OwnerResourceTypeMappingExtensions()
        {
            Mapping = new EnumMap<OwnerResourceType, RelatedEntityType>
            (
                (OwnerResourceType.ItContract, RelatedEntityType.itContract),
                (OwnerResourceType.ItSystemUsage, RelatedEntityType.itSystemUsage),
                (OwnerResourceType.DataProcessingRegistration, RelatedEntityType.dataProcessingRegistration)
            );
        }

        public static RelatedEntityType ToRelatedEntityType(this OwnerResourceType value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static OwnerResourceType ToOwnerResourceType(this RelatedEntityType value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}