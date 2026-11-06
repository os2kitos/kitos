using Core.Abstractions.Types;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public static class SystemRelationDirectionMappingExtensions
    {
        private static readonly EnumMap<SystemRelationDirectionChoice, SystemRelationDirection> Mapping =
            new(
                (SystemRelationDirectionChoice.IncomingData, SystemRelationDirection.IncomingData),
                (SystemRelationDirectionChoice.OutgoingData, SystemRelationDirection.OutgoingData),
                (SystemRelationDirectionChoice.BidirectionalData, SystemRelationDirection.BidirectionalData));

        public static SystemRelationDirection ToSystemRelationDirection(this SystemRelationDirectionChoice value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static SystemRelationDirectionChoice ToSystemRelationDirectionChoice(this SystemRelationDirection value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}
