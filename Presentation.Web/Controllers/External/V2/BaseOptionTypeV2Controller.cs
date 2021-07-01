using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel;
using Presentation.Web.Extensions;
using Presentation.Web.Models.External.V2;
using Presentation.Web.Models.External.V2.Request;
using Presentation.Web.Models.External.V2.Response;

namespace Presentation.Web.Controllers.External.V2
{
    public abstract class BaseOptionTypeV2Controller<TParent,TOption> : ExternalBaseController where TOption : OptionEntity<TParent>
    {
        private readonly IOptionsApplicationService<TParent, TOption> _optionApplicationService;

        protected BaseOptionTypeV2Controller(IOptionsApplicationService<TParent, TOption> optionApplicationService)
        {
            _optionApplicationService = optionApplicationService;
        }

        protected IHttpActionResult GetAll(Guid organizationUuid, [FromUri] StandardPaginationQuery pagination = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _optionApplicationService
                .GetOptionTypes(organizationUuid)
                .Select(x => x.Page(pagination))
                .Select(ToDTOs)
                .Match(Ok, FromOperationError);
        }

        protected IHttpActionResult GetSingle(Guid optionUuid, Guid organizationUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _optionApplicationService
                .GetOptionType(organizationUuid, optionUuid)
                .Select(x => ToAvailableDTO(x.option, x.available))
                .Match(Ok, FromOperationError);
        }

        private static List<IdentityNamePairResponseDTO> ToDTOs(IEnumerable<OptionEntity<TParent>> options)
        {
            return options.Select(ToDTO).ToList();
        }

        private static IdentityNamePairResponseDTO ToDTO(OptionEntity<TParent> option)
        {
            return new(option.Uuid, option.Name);
        }

        private static AvailableNamePairResponseDTO ToAvailableDTO(OptionEntity<TParent> option, bool isAvailable)
        {
            return new(option.Uuid, option.Name, isAvailable);
        }
    }
}