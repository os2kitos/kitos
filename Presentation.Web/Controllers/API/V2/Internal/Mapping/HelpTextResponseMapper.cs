using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Internal.Response;

namespace Presentation.Web.Controllers.API.V2.Internal.Mapping
{
    public class HelpTextResponseMapper: IHelpTextResponseMapper
    {
        public IEnumerable<HelpTextResponseDTO> ToResponseDTOs(IEnumerable<HelpText> helpTexts)
        {
            return helpTexts.Select(ToResponseDTO);
        }

        public HelpTextResponseDTO ToResponseDTO(HelpText helpText)
        {
            return new()
            {
                Description = helpText.Description,
                Key = helpText.Key,
                Title = helpText.Title
            };
        }
    }
}