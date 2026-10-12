using System.Collections.Generic;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Internal.Response;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Web.Controllers.API.V2.Internal.Mapping
{
    public interface IHelpTextResponseMapper
    {
        IEnumerable<HelpTextResponseDTO> ToResponseDTOs(IEnumerable<HelpText> helpTexts);
        HelpTextResponseDTO ToResponseDTO(HelpText helpText);

    }
}
