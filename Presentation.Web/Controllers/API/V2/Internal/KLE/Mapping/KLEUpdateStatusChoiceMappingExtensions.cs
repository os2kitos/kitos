using Core.Abstractions.Types;
using Core.DomainServices.Repositories.KLE;
using Presentation.Web.Models.API.V2.Internal.Response.KLE;

namespace Presentation.Web.Controllers.API.V2.Internal.KLE.Mapping
{
    public static class KLEUpdateStatusChoiceMappingExtensions
    {
        private static readonly EnumMap<KLEUpdateStatusChoice, KLEUpdateStatus> Mapping;

        static KLEUpdateStatusChoiceMappingExtensions()
        {
            Mapping = new EnumMap<KLEUpdateStatusChoice, KLEUpdateStatus>
            (
                (KLEUpdateStatusChoice.Failed, KLEUpdateStatus.Failed),
                (KLEUpdateStatusChoice.Ok, KLEUpdateStatus.Ok)
            );
        }

        public static KLEUpdateStatus ToStatus(this KLEUpdateStatusChoice value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static KLEUpdateStatusChoice ToChoice(this KLEUpdateStatus value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}