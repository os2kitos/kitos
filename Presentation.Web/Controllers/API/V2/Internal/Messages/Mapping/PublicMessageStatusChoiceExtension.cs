using Core.Abstractions.Types;
using Core.DomainModel.PublicMessage;
using Presentation.Web.Models.API.V2.Internal.Response;

namespace Presentation.Web.Controllers.API.V2.Internal.Messages.Mapping
{
    public static class PublicMessageStatusChoiceExtension
    {
        private static readonly EnumMap<PublicMessageStatusChoice, PublicMessageStatus> Mapping;
        static PublicMessageStatusChoiceExtension()
        {
            Mapping = new EnumMap<PublicMessageStatusChoice, PublicMessageStatus>
            (
                (PublicMessageStatusChoice.Active, PublicMessageStatus.Active),
                (PublicMessageStatusChoice.Inactive, PublicMessageStatus.Inactive)
            );
        }

        public static PublicMessageStatus ToPublicMessageStatus(this PublicMessageStatusChoice value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static PublicMessageStatusChoice ToPublicMessageStatusChoice(this PublicMessageStatus value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}