using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Internal.Response;

namespace Presentation.Web.Controllers.API.V2.Internal.Messages.Mapping
{
    public static class PublicMessagesResponseMapper
    {
        public static PublicMessagesResponseDTO ToTDO(this IEnumerable<Text> texts)
        {
            return texts.Aggregate(new PublicMessagesResponseDTO(), (dto, text) => MapText(text, dto));
        }

        private static PublicMessagesResponseDTO MapText(Text text, PublicMessagesResponseDTO dto)
        {
            var textValue = text.Value ?? "";
            switch (text.Id)
            {
                case 1:
                    dto.About = textValue;
                    break;
                case 2:
                    dto.Misc = textValue;
                    break;
                case 3:
                    dto.Guides = textValue;
                    break;
                case 4:
                    dto.StatusMessages = textValue;
                    break;
                case 5:
                    dto.ContactInfo = textValue;
                    break;
            }

            return dto;
        }
    }
}