using Core.ApplicationServices.GDPR;
using Core.DomainModel.GDPR;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.DPR;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.GDPR.Write;
using Presentation.Web.Controllers.API.V2.External.DataProcessingRegistrations.Mapping;
using Presentation.Web.Models.API.V2.Request.DataProcessing;
using Presentation.Web.Models.API.V2.Response.DataProcessing;
using Presentation.Web.Models.API.V2.Types.Shared;
using Presentation.Web.Models.API.V2.Response.Shared;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Web.Controllers.API.V2.External.DataProcessingRegistrations
{
    /// <summary>
    /// API for the data processing registrations in KITOS
    /// </summary>
    [Route("api/v2/data-processing-registrations")]
    public class DataProcessingRegistrationV2Controller : ExternalBaseController
    {
        private readonly IDataProcessingRegistrationApplicationService _dataProcessingRegistrationService;
        private readonly IDataProcessingRegistrationWriteService _writeService;
        private readonly IDataProcessingRegistrationWriteModelMapper _writeModelMapper;
        private readonly IDataProcessingRegistrationResponseMapper _responseMapper;
        private readonly IResourcePermissionsResponseMapper _permissionResponseMapper;

        public DataProcessingRegistrationV2Controller(
            IDataProcessingRegistrationApplicationService dataProcessingRegistrationService,
            IDataProcessingRegistrationWriteService writeService,
            IDataProcessingRegistrationWriteModelMapper writeModelMapper,
            IDataProcessingRegistrationResponseMapper responseMapper, IResourcePermissionsResponseMapper permissionResponseMapper)
        {
            _dataProcessingRegistrationService = dataProcessingRegistrationService;
            _writeService = writeService;
            _writeModelMapper = writeModelMapper;
            _responseMapper = responseMapper;
            _permissionResponseMapper = permissionResponseMapper;
        }

        /// <summary>
        /// Returns all Data-Processing-Registrations available to the user
        /// </summary>
        /// <param name="organizationUuid">Organization UUID filter</param>
        /// <param name="systemUuid">System UUID filter</param>
        /// <param name="systemUsageUuid">SystemUsage UUID filter</param>
        /// <param name="dataProcessorUuid">UUID of a data processor in the registration</param>
        /// <param name="subDataProcessorUuid">UUID of a sub data processor in the registration</param>
        /// <param name="agreementConcluded">Filter based on whether or not an agreement has been concluded</param>
        /// <param name="changedSinceGtEq">Include only changes which were LastModified (UTC) is equal to or greater than the provided value</param>
        /// <param name="orderByProperty">Ordering property</param>
        /// <param name="nameContains">Filter based on whether the name contains the provided value</param>
        /// <param name="nameEquals">Filter based on whether the name equals the provided value</param>
        /// <param name="paginationQuery">Pagination query parameters</param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        [ApiResponse(typeof(IEnumerable<DataProcessingRegistrationResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult GetDataProcessingRegistrations(
            [NonEmptyGuid] Guid? organizationUuid = null,
            [NonEmptyGuid] Guid? systemUuid = null,
            [NonEmptyGuid] Guid? systemUsageUuid = null,
            [NonEmptyGuid] Guid? dataProcessorUuid = null,
            [NonEmptyGuid] Guid? subDataProcessorUuid = null,
            bool? agreementConcluded = null,
            string? nameContains = null,
            string? nameEquals = null,
            DateTime? changedSinceGtEq = null,
            CommonOrderByProperty? orderByProperty = null,
            [FromQuery] BoundedPaginationQuery? paginationQuery = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var conditions = new List<IDomainQuery<DataProcessingRegistration>>();

            if (organizationUuid.HasValue)
                conditions.Add(new QueryByOrganizationUuid<DataProcessingRegistration>(organizationUuid.Value));

            if (systemUuid.HasValue)
                conditions.Add(new QueryBySystemUuid(systemUuid.Value));

            if (systemUsageUuid.HasValue)
                conditions.Add(new QueryBySystemUsageUuid(systemUsageUuid.Value));

            if (dataProcessorUuid.HasValue)
                conditions.Add(new QueryByDataProcessorUuid(dataProcessorUuid.Value));

            if (subDataProcessorUuid.HasValue)
                conditions.Add(new QueryBySubDataProcessorUuid(subDataProcessorUuid.Value));

            if (agreementConcluded.HasValue)
                conditions.Add(new QueryByAgreementConcluded(agreementConcluded.Value));

            if(!string.IsNullOrWhiteSpace(nameContains))
                conditions.Add(new QueryByPartOfName<DataProcessingRegistration>(nameContains));

            if(!string.IsNullOrWhiteSpace(nameEquals))
                conditions.Add(new QueryByName<DataProcessingRegistration>(nameEquals));

            if (changedSinceGtEq.HasValue)
                conditions.Add(new QueryByChangedSinceGtEq<DataProcessingRegistration>(changedSinceGtEq.Value));

            return _dataProcessingRegistrationService
                .Query(conditions.ToArray())
                .Include(registration => registration.Organization)
                .Include(registration => registration.ObjectOwner)
                .Include(registration => registration.LastChangedByUser)
                .Include(registration => registration.SystemUsages)
                .Include(registration => registration.OversightOptions)
                .Include(registration => registration.Rights)
                    .ThenInclude(right => right.Role)
                .Include(registration => registration.Rights)
                    .ThenInclude(right => right.User)
                .Include(registration => registration.ExternalReferences)
                .Include(registration => registration.DataResponsible)
                .Include(registration => registration.BasisForTransfer)
                .Include(registration => registration.InsecureCountriesSubjectToDataTransfer)
                .Include(registration => registration.DataProcessors)
                .Include(registration => registration.AssignedSubDataProcessors)
                    .ThenInclude(subDataProcessor => subDataProcessor.Organization)
                .Include(registration => registration.AssignedSubDataProcessors)
                    .ThenInclude(subDataProcessor => subDataProcessor.SubDataProcessorBasisForTransfer)
                .Include(registration => registration.AssignedSubDataProcessors)
                    .ThenInclude(subDataProcessor => subDataProcessor.InsecureCountry)
                .Include(registration => registration.MainContract)
                .Include(registration => registration.AssociatedContracts)
                .Include(registration => registration.ResponsibleOrganizationUnit)
                .Include(registration => registration.OversightDates)
                .AsSplitQuery()
                .OrderApiResultsByDefaultConventions(changedSinceGtEq.HasValue, orderByProperty)
                .Page(paginationQuery)
                .ToList()
                .Select(_responseMapper.MapDataProcessingRegistrationDTO)
                .ToList()
                .Transform(Ok);
        }

        /// <summary>
        /// Returns a specific Data-Processing-Registration
        /// </summary>
        /// <param name="uuid">UUID of Data-Processing-Registration entity</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{uuid}")]
        [ApiResponse(typeof(DataProcessingRegistrationResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult GetDataProcessingRegistration([NonEmptyGuid] Guid uuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _dataProcessingRegistrationService
                .GetByUuid(uuid)
                .Select(_responseMapper.MapDataProcessingRegistrationDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Create a new data processing registration
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        [ApiResponse(typeof(DataProcessingRegistrationResponseDTO), HttpStatusCode.Created)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Conflict)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult PostDataProcessingRegistration([FromBody] CreateDataProcessingRegistrationRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _writeService
                .Create(request.OrganizationUuid, _writeModelMapper.FromPOST(request))
                .Select(_responseMapper.MapDataProcessingRegistrationDTO)
                .Match(MapCreatedResponse, FromOperationError);
        }

        /// <summary>
        /// Perform a full update of an existing data processing registration.
        /// Note: PUT expects a full version of the updated registration. For partial updates, please use PATCH.
        /// </summary>
        /// <param name="uuid">UUID of the data processing registration</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{uuid}")]
        [ApiResponse(typeof(DataProcessingRegistrationResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Conflict)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult PutDataProcessingRegistration([NonEmptyGuid] Guid uuid, [FromBody] UpdateDataProcessingRegistrationRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _writeService
                .Update(uuid, _writeModelMapper.FromPUT(request))
                .Select(_responseMapper.MapDataProcessingRegistrationDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Allows partial updates of an existing data processing registration using json merge patch semantics (RFC7396)
        /// </summary>
        /// <param name="uuid">UUID of the data processing registration</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{uuid}")]
        [ApiResponse(typeof(DataProcessingRegistrationResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Conflict)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult PatchDataProcessingRegistration([NonEmptyGuid] Guid uuid, [FromBody] UpdateDataProcessingRegistrationRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _writeService
                .Update(uuid, _writeModelMapper.FromPATCH(request))
                .Select(_responseMapper.MapDataProcessingRegistrationDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Removes an existing data processing registration.
        /// </summary>
        /// <param name="uuid">UUID of the data processing registration</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{uuid}")]
        [ApiResponse(HttpStatusCode.NoContent)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult DeleteDataProcessingRegistration([NonEmptyGuid] Guid uuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _writeService
                .Delete(uuid)
                .Match(FromOperationError, () => StatusCode((int)HttpStatusCode.NoContent));
        }

        /// <summary>
        /// Returns the permissions of the authenticated client in the context of a specific Data Processing Registration
        /// </summary>
        /// <param name="dprUuid">UUID of the contract entity</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{dprUuid}/permissions")]
        [ApiResponse(typeof(CombinedPermissionsResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetDataProcessingRegistrationPermissions([NonEmptyGuid] Guid dprUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _dataProcessingRegistrationService
                .GetPermissions(dprUuid)
                .Select(_permissionResponseMapper.Map)
                .Match(Ok, FromOperationError);
        }


        /// <summary>
        /// Returns the permissions of the authenticated client for the Data Processing Registration resources collection in the context of an organization (Data Processing Registration permissions in a specific Organization)
        /// </summary>
        /// <param name="organizationUuid">UUID of the organization</param>
        /// <returns></returns>
        [HttpGet]
        [Route("permissions")]
        [ApiResponse(typeof(ResourceCollectionPermissionsResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetDataProcessingRegistrationCollectionPermissions([Required][NonEmptyGuid] Guid organizationUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _dataProcessingRegistrationService.GetCollectionPermissions(organizationUuid)
                .Select(_permissionResponseMapper.Map)
                .Match(Ok, FromOperationError);
        }
        
        private IActionResult MapCreatedResponse(DataProcessingRegistrationResponseDTO dto)
        {
            return Created($"{new Uri($"{Request.Scheme}://{Request.Host}{Request.Path}").AbsoluteUri.TrimEnd('/')}/{dto.Uuid}", dto);
        }
    }
}



