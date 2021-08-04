using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel;
using Presentation.Web.Extensions;
using Presentation.Web.Models.API.V2.Request;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;

namespace Presentation.Web.Controllers.API.V2.External
{
    public abstract class BaseOptionTypeV2Controller<TParent,TOption, TCollectionEntryDTO, TExtendedDto> : ExternalBaseController where TOption : OptionEntity<TParent>
    {
        private readonly IOptionsApplicationService<TParent, TOption> _optionApplicationService;

        protected BaseOptionTypeV2Controller(IOptionsApplicationService<TParent, TOption> optionApplicationService)
        {
            _optionApplicationService = optionApplicationService;
        }

        protected IHttpActionResult GetAll(Guid organizationUuid, [FromUri] UnboundedPaginationQuery pagination = null)
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
                .Select(x => ToExtendedDTO(x.option, x.available))
                .Match(Ok, FromOperationError);
        }

        private List<TCollectionEntryDTO> ToDTOs(IEnumerable<TOption> options)
        {
            return options.Select(ToDTO).ToList();
        }

        protected abstract TCollectionEntryDTO ToDTO(TOption option);

        protected abstract TExtendedDto ToExtendedDTO(TOption option, bool isAvailable);
    }
}