
using Core.ApplicationServices.Contract;
using Core.DomainModel.ItContract;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.Contract;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Response.Contract;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using Core.ApplicationServices.Contract.Write;
using Presentation.Web.Controllers.API.V2.External.ItContracts.Mapping;
using Presentation.Web.Models.API.V2.Request.Contract;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Core.Abstractions.Extensions;

namespace Presentation.Web.Controllers.API.V2.External.ItContracts
{
    /// <summary>
    /// API for the contracts stored in KITOS.
    /// </summary>
    [RoutePrefix("api/v2/it-contracts")]
    public class ItContractV2Controller : ExternalBaseController
    {
        private readonly IItContractService _itContractService;
        private readonly IItContractResponseMapper _responseMapper;
        private readonly IItContractWriteModelMapper _writeModelMapper;
        private readonly IItContractWriteService _writeService;

        public ItContractV2Controller(IItContractService itContractService, IItContractResponseMapper responseMapper, IItContractWriteModelMapper writeModelMapper, IItContractWriteService writeService)
        {
            _itContractService = itContractService;
            _responseMapper = responseMapper;
            _writeModelMapper = writeModelMapper;
            _writeService = writeService;
        }

        /// <summary>
        /// Returns all IT-Contracts available to the current user within the provided organization context
        /// </summary>
        /// <param name="organizationUuid">Organization UUID filter</param>
        /// <param name="systemUuid">Associated system UUID filter</param>
        /// <param name="systemUsageUuid">Associated system usage UUID filter</param>
        /// <param name="dataProcessingRegistrationUuid">Associated data processing registration UUID filter</param>
        /// <param name="nameContent">Name content filter</param>
        /// <param name="changedSinceGtEq">Include only changes which were LastModified (UTC) is equal to or greater than the provided value</param>
        /// <returns></returns>
        [HttpGet]
        [Route]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ItContractResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItContracts(
            [NonEmptyGuid] Guid? organizationUuid = null,
            [NonEmptyGuid] Guid? systemUuid = null,
            [NonEmptyGuid] Guid? systemUsageUuid = null,
            [NonEmptyGuid] Guid? dataProcessingRegistrationUuid = null,
            [NonEmptyGuid] Guid? responsibleOrgUnitUuid = null,
            [NonEmptyGuid] Guid? supplierUuid = null,
            string nameContent = null,
            DateTime? changedSinceGtEq = null,
            [FromUri] BoundedPaginationQuery paginationQuery = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var conditions = new List<IDomainQuery<ItContract>>();

            if (organizationUuid.HasValue)
                conditions.Add(new QueryByOrganizationUuid<ItContract>(organizationUuid.Value));

            if (systemUuid.HasValue)
                conditions.Add(new QueryBySystemUuid(systemUuid.Value));

            if (systemUsageUuid.HasValue)
                conditions.Add(new QueryBySystemUsageUuid(systemUsageUuid.Value));

            if (dataProcessingRegistrationUuid.HasValue)
                conditions.Add(new QueryByDataProcessingRegistrationUuid(dataProcessingRegistrationUuid.Value));

            if (responsibleOrgUnitUuid.HasValue)
                conditions.Add(new QueryByResponsibleOrganizationUnitUuid(responsibleOrgUnitUuid.Value));

            if (supplierUuid.HasValue)
                conditions.Add(new QueryBySupplierUuid(supplierUuid.Value));

            if (!string.IsNullOrWhiteSpace(nameContent))
                conditions.Add(new QueryByPartOfName<ItContract>(nameContent));

            if (changedSinceGtEq.HasValue)
                conditions.Add(new QueryByChangedSinceGtEq<ItContract>(changedSinceGtEq.Value));

            return _itContractService
                .Query(conditions.ToArray())
                .OrderByDefaultConventions(changedSinceGtEq.HasValue)
                .Page(paginationQuery)
                .ToList()
                .Select(_responseMapper.MapContractDTO)
                .Transform(Ok);
        }

        /// <summary>
        /// Returns requested IT-Contract
        /// </summary>
        /// <param name="uuid">Specific IT-Contract UUID</param>
        /// <returns>Specific data related to the IT-Contract</returns>
        [HttpGet]
        [Route("{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItContractResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetItContract([NonEmptyGuid] Guid uuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _itContractService
                .GetContract(uuid)
                .Select(_responseMapper.MapContractDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Creates a new it-contract in the requested organization
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(ItContractResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public IHttpActionResult PostItContract([FromBody] CreateNewContractRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var parameters = _writeModelMapper.FromPOST(request);

            return _writeService
                .Create(request.OrganizationUuid, parameters)
                .Select(_responseMapper.MapContractDTO)
                .Match(MapCreatedResponse, FromOperationError);
        }

        /// <summary>
        /// Updates an existing it-contract
        /// Note: PUT expects a full version of the updated contract. For partial updates, please use PATCH.
        /// </summary>
        /// <param name="contractUuid">UUID of the contract in KITOS</param>
        /// <param name="request">Full update of the contract</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{contractUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItContractResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult PutItContract([NonEmptyGuid] Guid contractUuid, [FromBody] UpdateContractRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var parameters = _writeModelMapper.FromPUT(request);

            return _writeService
                .Update(contractUuid, parameters)
                .Select(_responseMapper.MapContractDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Allows partial updates of an existing it-contract
        /// NOTE:At the root level, defined sections will be mapped as changes e.g. {General: null} will reset the entire "General" section.
        /// If the section is not provided in the json, the omitted section will remain unchanged.
        /// At the moment we only manage PATCH at the root level so all levels below that must be provided in it's entirety 
        /// </summary>
        /// <param name="contractUuid">UUID of the contract in KITOS</param>
        /// <param name="request">Full update of the contract</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{contractUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItContractResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult PatchItContract([NonEmptyGuid] Guid contractUuid, [FromBody] UpdateContractRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var parameters = _writeModelMapper.FromPATCH(request);

            return _writeService
                .Update(contractUuid, parameters)
                .Select(_responseMapper.MapContractDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Delete an existing contract
        /// </summary>
        /// <param name="contractUuid"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{contractUuid}")]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult DeleteItContract([NonEmptyGuid] Guid contractUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _writeService
                .Delete(contractUuid)
                .Match(FromOperationError, () => StatusCode(HttpStatusCode.NoContent));
        }

        private CreatedNegotiatedContentResult<ItContractResponseDTO> MapCreatedResponse(ItContractResponseDTO dto)
        {
            return Created($"{Request.RequestUri.AbsoluteUri.TrimEnd('/')}/{dto.Uuid}", dto);
        }
    }
}