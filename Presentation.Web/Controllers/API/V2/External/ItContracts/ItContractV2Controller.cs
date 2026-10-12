
using Core.ApplicationServices.Contract;
using Core.DomainModel.ItContract;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.Contract;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Response.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Core.ApplicationServices.Contract.Write;
using Presentation.Web.Controllers.API.V2.External.ItContracts.Mapping;
using Presentation.Web.Models.API.V2.Request.Contract;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Core.Abstractions.Extensions;
using Presentation.Web.Models.API.V2.Types.Shared;
using Presentation.Web.Models.API.V2.Response.Shared;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Microsoft.AspNetCore.Mvc;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Web.Controllers.API.V2.External.ItContracts
{
    /// <summary>
    /// API for the contracts stored in KITOS.
    /// </summary>
    [Route("api/v2/it-contracts")]
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
        /// <param name="paginationQuery">Pagination parameters</param>
        /// <param name="responsibleOrgUnitUuid">Responsible organization unit UUID filter</param>
        /// <param name="supplierUuid">Supplier UUID filter</param>
        /// <param name="nameEquals">Exact name filter</param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        [ApiResponse(typeof(IEnumerable<ItContractResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult GetItContracts(
            [NonEmptyGuid] Guid? organizationUuid = null,
            [NonEmptyGuid] Guid? systemUuid = null,
            [NonEmptyGuid] Guid? systemUsageUuid = null,
            [NonEmptyGuid] Guid? dataProcessingRegistrationUuid = null,
            [NonEmptyGuid] Guid? responsibleOrgUnitUuid = null,
            [NonEmptyGuid] Guid? supplierUuid = null,
            string? nameContent = null,
            string? nameEquals = null,
            DateTime? changedSinceGtEq = null,
            CommonOrderByProperty? orderByProperty = null,
            [FromQuery] BoundedPaginationQuery? paginationQuery = null)
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
                .Include(contract => contract.Parent)
                .Include(contract => contract.Organization)
                .Include(contract => contract.ObjectOwner)
                .Include(contract => contract.LastChangedByUser)
                .Include(contract => contract.Supplier)
                .Include(contract => contract.ResponsibleOrganizationUnit)
                    .ThenInclude(unit => unit.Parent)
                .Include(contract => contract.ProcurementStrategy)
                .Include(contract => contract.PurchaseForm)
                .Include(contract => contract.ContractTemplate)
                .Include(contract => contract.ContractType)
                .Include(contract => contract.Criticality)
                .Include(contract => contract.OptionExtend)
                .Include(contract => contract.TerminationDeadline)
                .Include(contract => contract.PaymentModel)
                .Include(contract => contract.PaymentFreqency)
                .Include(contract => contract.PriceRegulation)
                .Include(contract => contract.AssociatedSystemUsages)
                    .ThenInclude(association => association.ItSystemUsage)
                .Include(contract => contract.DataProcessingRegistrations)
                .Include(contract => contract.AssociatedAgreementElementTypes)
                    .ThenInclude(element => element.AgreementElementType)
                .Include(contract => contract.Rights)
                    .ThenInclude(right => right.Role)
                .Include(contract => contract.Rights)
                    .ThenInclude(right => right.User)
                .Include(contract => contract.ExternEconomyStreams)
                    .ThenInclude(stream => stream.OrganizationUnit)
                        .ThenInclude(unit => unit.Parent)
                .Include(contract => contract.InternEconomyStreams)
                    .ThenInclude(stream => stream.OrganizationUnit)
                        .ThenInclude(unit => unit.Parent)
                .Include(contract => contract.ExternalReferences)
                .AsSplitQuery()
                .OrderApiResultsByDefaultConventions(changedSinceGtEq.HasValue, orderByProperty)
                .Page(paginationQuery)
                .ToList()
                .Select(_responseMapper.MapContractDTO)
                .Transform(Ok);
        }

        /// <summary>
        /// Returns requested IT-Contract
        /// </summary>
        /// <param name="contractUuid">Specific IT-Contract UUID</param>
        /// <returns>Specific data related to the IT-Contract</returns>
        [HttpGet]
        [Route("{contractUuid}")]
        [ApiResponse(typeof(ItContractResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetItContract([NonEmptyGuid] Guid contractUuid)
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
        [Route("")]
        [ApiResponse(typeof(ItContractResponseDTO), HttpStatusCode.Created)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.Conflict)]
        public IActionResult PostItContract([FromBody] CreateNewContractRequestDTO request)
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
        [ApiResponse(typeof(ItContractResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.Conflict)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult PutItContract([NonEmptyGuid] Guid contractUuid, [FromBody] UpdateContractRequestDTO request)
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
        [ApiResponse(typeof(ItContractResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.Conflict)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult PatchItContract([NonEmptyGuid] Guid contractUuid, [FromBody] UpdateContractRequestDTO request)
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
        /// <param name="contractUuid">UUID of the contract in KITOS</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{contractUuid}")]
        [ApiResponse(HttpStatusCode.NoContent)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult DeleteItContract([NonEmptyGuid] Guid contractUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _writeService
                .Delete(contractUuid)
                .Match(FromOperationError, () => StatusCode((int)HttpStatusCode.NoContent));
        }

        /// <summary>
        /// Returns the permissions of the authenticated client in the context of a specific IT-Contract
        /// </summary>
        /// <param name="contractUuid">UUID of the contract entity</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{contractUuid}/permissions")]
        [ApiResponse(typeof(ItContractPermissionsResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetItContractPermissions([NonEmptyGuid] Guid contractUuid)
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
        [ApiResponse(typeof(ResourceCollectionPermissionsResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetItContractCollectionPermissions([Required][NonEmptyGuid] Guid organizationUuid)
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
        /// <param name="contractUuid">UUID of the contract</param>
        /// <param name="dto">External reference data</param>
        /// <returns></returns>
        [HttpPost]
        [Route("{contractUuid}/external-references")]
        [ApiResponse(typeof(ExternalReferenceDataResponseDTO), HttpStatusCode.Created)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult PostExternalReference([NonEmptyGuid] Guid contractUuid, [FromBody] ExternalReferenceDataWriteRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var properties = _writeModelMapper.MapExternalReference(dto);

            return _writeService
                .AddExternalReference(contractUuid, properties)
                .Select(_referenceResponseMapper.MapExternalReference)
                .Match(reference => Created($"{new Uri($"{Request.Scheme}://{Request.Host}{Request.Path}").AbsoluteUri.TrimEnd('/')}/{contractUuid}/external-references/{reference.Uuid}", reference), FromOperationError);
        }

        /// <summary>
        /// Updates a contract external reference
        /// </summary>
        /// <param name="contractUuid"></param>
        /// <param name="externalReferenceUuid"></param>
        /// <param name="dto">External reference data</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{contractUuid}/external-references/{externalReferenceUuid}")]
        [ApiResponse(typeof(ExternalReferenceDataResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult PutExternalReference([NonEmptyGuid] Guid contractUuid, [NonEmptyGuid] Guid externalReferenceUuid, [FromBody] ExternalReferenceDataWriteRequestDTO dto)
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
        [ApiResponse(HttpStatusCode.NoContent)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult DeleteExternalReference([NonEmptyGuid] Guid contractUuid, [NonEmptyGuid] Guid externalReferenceUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _writeService
                .DeleteExternalReference(contractUuid, externalReferenceUuid)
                .Match(_ => NoContent(), FromOperationError);
        }

        private IActionResult MapCreatedResponse(ItContractResponseDTO dto)
        {
            return Created($"{new Uri($"{Request.Scheme}://{Request.Host}{Request.Path}").AbsoluteUri.TrimEnd('/')}/{dto.Uuid}", dto);
        }
    }
}



