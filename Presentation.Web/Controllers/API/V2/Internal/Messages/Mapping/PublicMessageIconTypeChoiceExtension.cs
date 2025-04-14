using Core.Abstractions.Types;
using Core.DomainModel.PublicMessage;
using Presentation.Web.Models.API.V2.Internal.Response;

namespace Presentation.Web.Controllers.API.V2.Internal.Messages.Mapping
{
    public static class PublicMessageIconTypeChoiceExtension
    {
        private static readonly EnumMap<PublicMessageIconTypeChoice, PublicMessageIconType> Mapping;
        static PublicMessageIconTypeChoiceExtension()
        {
            Mapping = new EnumMap<PublicMessageIconTypeChoice, PublicMessageIconType>
            (
                (PublicMessageIconTypeChoice.Document, PublicMessageIconType.Document),
                (PublicMessageIconTypeChoice.Clipboard, PublicMessageIconType.Clipboard),
                (PublicMessageIconTypeChoice.Settings, PublicMessageIconType.Settings),
                (PublicMessageIconTypeChoice.Calendar, PublicMessageIconType.Calendar),
                (PublicMessageIconTypeChoice.MultipleUsers, PublicMessageIconType.MultipleUsers),
                (PublicMessageIconTypeChoice.Mail, PublicMessageIconType.Mail)
            );
        }

        public static PublicMessageIconType ToPublicMessageIconType(this PublicMessageIconTypeChoice value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static PublicMessageIconTypeChoice ToPublicMessageIconTypeChoice(this PublicMessageIconType value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}