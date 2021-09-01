using Core.ApplicationServices.Contract;
using Core.DomainModel.ItContract;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.Contract;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request;
using Presentation.Web.Models.API.V2.Response.Contract;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Presentation.Web.Controllers.API.V2.Mapping;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;

namespace Presentation.Web.Controllers.API.V2.External.ItContracts
{
    /// <summary>
    /// API for the contracts stored in KITOS.
    /// </summary>
    [RoutePrefix("api/v2")]
    public class ItContractV2Controller : ExternalBaseController
    {
        private readonly IItContractService _itContractService;

        public ItContractV2Controller(IItContractService itContractService)
        {
            _itContractService = itContractService;
        }

        /// <summary>
        /// Returns all IT-Contracts available to the current user within the provided organization context
        /// </summary>
        /// <param name="organizationUuid">Organization UUID filter</param>
        /// <param name="systemUuid">Associated system UUID filter</param>
        /// <param name="systemUsageUuid">Associated system usage UUID filter</param>
        /// <param name="nameContent">Name content filter</param>
        /// <returns></returns>
        [HttpGet]
        [Route("it-contracts")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ItContractResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItContracts(
            [NonEmptyGuid] Guid organizationUuid,
            [NonEmptyGuid] Guid? systemUuid = null,
            [NonEmptyGuid] Guid? systemUsageUuid = null,
            string nameContent = null,
            [FromUri] BoundedPaginationQuery paginationQuery = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var refinements = new List<IDomainQuery<ItContract>>();

            if (systemUuid.HasValue)
                refinements.Add(new QueryBySystemUuid(systemUuid.Value));

            if (systemUsageUuid.HasValue)
                refinements.Add(new QueryBySystemUsageUuid(systemUsageUuid.Value));

            if (!string.IsNullOrWhiteSpace(nameContent))
                refinements.Add(new QueryByPartOfName<ItContract>(nameContent));

            return _itContractService
                .GetContractsInOrganization(organizationUuid, refinements.ToArray())
                .Select(x => x.OrderBy(contract => contract.Id))
                .Select(x => x.Page(paginationQuery))
                .Select(x => x.ToList().Select(ToItContractResponseDto).ToList())
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Returns requested IT-Contract
        /// </summary>
        /// <param name="uuid">Specific IT-Contract UUID</param>
        /// <returns>Specific data related to the IT-Contract</returns>
        [HttpGet]
        [Route("it-contracts/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItContractResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetItProject([NonEmptyGuid] Guid uuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _itContractService
                .GetContract(uuid)
                .Select(ToItContractResponseDto)
                .Match(Ok, FromOperationError);
        }

        private static ItContractResponseDTO ToItContractResponseDto(ItContract contract)
        {
            return new()
            {
                Uuid = contract.Uuid,
                Name = contract.Name,
                ContractType = contract.ContractTypeId.HasValue ? contract.ContractType.MapIdentityNamePairDTO() : null,
                Supplier = contract.SupplierId.HasValue ? contract.Supplier.MapShallowOrganizationResponseDTO() : null,
                AgreementElements = contract.AssociatedAgreementElementTypes.Select(x => x.AgreementElementType.MapIdentityNamePairDTO()),
                ValidFrom = contract.Concluded,
                ValidTo = contract.ExpirationDate,
                TerminatedAt = contract.Terminated,
                IsValid = contract.IsActive
            };
        }
    }
}