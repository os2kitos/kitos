
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
using Presentation.Web.Models.API.V2.Types.Shared;
using Presentation.Web.Models.API.V2.Response.Shared;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;

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
        private readonly IResourcePermissionsResponseMapper _permissionResponseMapper;
        private readonly IExternalReferenceResponseMapper _referenceResponseMapper;

        public ItContractV2Controller(IItContractService itContractService,
            IItContractResponseMapper responseMapper,
            IItContractWriteModelMapper writeModelMapper,
            IItContractWriteService writeService,
            IResourcePermissionsResponseMapper permissionResponseMapper,
            IExternalReferenceResponseMapper referenceResponseMapper)
        {
            _itContractService = itContractService;
            _responseMapper = responseMapper;
            _writeModelMapper = writeModelMapper;
            _writeService = writeService;
            _permissionResponseMapper = permissionResponseMapper;
            _referenceResponseMapper = referenceResponseMapper;
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
        /// <param name="orderByProperty">Ordering property</param>
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
            string nameEquals = null,
            DateTime? changedSinceGtEq = null,
            CommonOrderByProperty? orderByProperty = null,
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

            if(!string.IsNullOrWhiteSpace(nameEquals))
                conditions.Add(new QueryByName<ItContract>(nameEquals));

            if (changedSinceGtEq.HasValue)
                conditions.Add(new QueryByChangedSinceGtEq<ItContract>(changedSinceGtEq.Value));


            return _itContractService
                .Query(conditions.ToArray())
                .OrderApiResultsByDefaultConventions(changedSinceGtEq.HasValue, orderByProperty)
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
        [Route("{contractUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItContractResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetItContract([NonEmptyGuid] Guid contractUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _itContractService
                .GetContract(contractUuid)
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
        [SwaggerResponseRemoveDefaults]
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
        /// Allows partial updates of an existing it-contract using json merge patch semantics (RFC7396)
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

        /// <summary>
        /// Returns the permissions of the authenticated client in the context of a specific IT-Contract
        /// </summary>
        /// <param name="contractUuid">UUID of the contract entity</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{contractUuid}/permissions")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItContractPermissionsResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetItContractPermissions([NonEmptyGuid] Guid contractUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _itContractService
                .GetPermissions(contractUuid)
                .Select(_responseMapper.MapPermissions)
                .Match(Ok, FromOperationError);
        }


        /// <summary>
        /// Returns the permissions of the authenticated client for the IT-Contract resources collection in the context of an organization (IT-Contract permissions in a specific Organization)
        /// </summary>
        /// <param name="organizationUuid">UUID of the organization</param>
        /// <returns></returns>
        [HttpGet]
        [Route("permissions")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ResourceCollectionPermissionsResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetItContractCollectionPermissions([Required][NonEmptyGuid] Guid organizationUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _itContractService.GetCollectionPermissions(organizationUuid)
                .Select(_permissionResponseMapper.Map)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Creates an external reference for the contract
        /// </summary>
        /// <param name="contractUuid"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{contractUuid}/external-references")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(ExternalReferenceDataResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PostExternalReference([NonEmptyGuid] Guid contractUuid, [FromBody] ExternalReferenceDataWriteRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var properties = _writeModelMapper.MapExternalReference(dto);

            return _writeService
                .AddExternalReference(contractUuid, properties)
                .Select(_referenceResponseMapper.MapExternalReference)
                .Match(reference => Created($"{Request.RequestUri.AbsoluteUri.TrimEnd('/')}/{contractUuid}/external-references/{reference.Uuid}", reference), FromOperationError);
        }

        /// <summary>
        /// Updates a contract external reference
        /// </summary>
        /// <param name="contractUuid"></param>
        /// <param name="externalReferenceUuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{contractUuid}/external-references/{externalReferenceUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ExternalReferenceDataResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PutExternalReference([NonEmptyGuid] Guid contractUuid, [NonEmptyGuid] Guid externalReferenceUuid, [FromBody] ExternalReferenceDataWriteRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var properties = _writeModelMapper.MapExternalReference(dto);

            return _writeService
                .UpdateExternalReference(contractUuid, externalReferenceUuid, properties)
                .Select(_referenceResponseMapper.MapExternalReference)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Deletes a contract external reference
        /// </summary>
        /// <param name="contractUuid"></param>
        /// <param name="externalReferenceUuid"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{contractUuid}/external-references/{externalReferenceUuid}")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult DeleteExternalReference([NonEmptyGuid] Guid contractUuid, [NonEmptyGuid] Guid externalReferenceUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _writeService
                .DeleteExternalReference(contractUuid, externalReferenceUuid)
                .Match(_ => NoContent(), FromOperationError);
        }

        private CreatedNegotiatedContentResult<ItContractResponseDTO> MapCreatedResponse(ItContractResponseDTO dto)
        {
            return Created($"{Request.RequestUri.AbsoluteUri.TrimEnd('/')}/{dto.Uuid}", dto);
        }
    }
}