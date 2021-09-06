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
using Presentation.Web.Controllers.API.V2.Mapping;
using Presentation.Web.Models.API.V2.Request.Contract;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Controllers.API.V2.External.ItContracts
{
    /// <summary>
    /// API for the contracts stored in KITOS.
    /// </summary>
    [RoutePrefix("api/v2/it-contracts")]
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
        /// <param name="dataProcessingRegistrationUuid">Associated data processing registration UUID filter</param>
        /// <param name="nameContent">Name content filter</param>
        /// <returns></returns>
        [HttpGet]
        [Route]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ItContractResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItContracts(
            [NonEmptyGuid] Guid organizationUuid,
            [NonEmptyGuid] Guid? systemUuid = null,
            [NonEmptyGuid] Guid? systemUsageUuid = null,
            [NonEmptyGuid] Guid? dataProcessingRegistrationUuid = null,
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

            if (dataProcessingRegistrationUuid.HasValue)
                refinements.Add(new QueryByDataProcessingRegistrationUuid(dataProcessingRegistrationUuid.Value));

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
        [Route("{uuid}")]
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
        public IHttpActionResult PostItProject([FromBody] CreateNewContractRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates an existing it-contract
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
        public IHttpActionResult PutItProject([NonEmptyGuid] Guid contractUuid, [FromBody] ContractWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates an existing it-contract's general data section
        /// </summary>
        /// <param name="contractUuid">UUID of the contract in KITOS</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{contractUuid}/general")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItContractResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult PutItProject([NonEmptyGuid] Guid contractUuid, [FromBody] ContractGeneralDataWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates an existing it-contract's procurement data section
        /// </summary>
        /// <param name="contractUuid">UUID of the contract in KITOS</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{contractUuid}/procurement")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItContractResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult PutItProjectGeneralData([NonEmptyGuid] Guid contractUuid, [FromBody] ContractProcurementDataWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates an existing it-contract's supplier data section
        /// </summary>
        /// <param name="contractUuid">UUID of the contract in KITOS</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{contractUuid}/supplier")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItContractResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult PutItProjectSupplierData([NonEmptyGuid] Guid contractUuid, [FromBody] ContractSupplierDataWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates an existing it-contract's responsible data section
        /// </summary>
        /// <param name="contractUuid">UUID of the contract in KITOS</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{contractUuid}/responsible")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItContractResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult PutItProjectResponsibleData([NonEmptyGuid] Guid contractUuid, [FromBody] ContractResponsibleDataWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates an existing it-contract's system usages
        /// </summary>
        /// <param name="contractUuid">UUID of the contract in KITOS</param>
        /// <param name="systemUsageUuids">Uuids of the system usages</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{contractUuid}/system-usages")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItContractResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult PutItProjectSystemUsages([NonEmptyGuid] Guid contractUuid, [FromBody] IEnumerable<Guid> systemUsageUuids)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates an existing it-contract's data processing registrations
        /// </summary>
        /// <param name="contractUuid">UUID of the contract in KITOS</param>
        /// <param name="dataProcessingRegistrationUuids">UUIDs of the data processing registrations</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{contractUuid}/data-processing-registrations")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItContractResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult PutItProjectDataProcessingRegistrations([NonEmptyGuid] Guid contractUuid, [FromBody] IEnumerable<Guid> dataProcessingRegistrationUuids)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates an existing it-contract's handover trial registrations
        /// </summary>
        /// <param name="contractUuid">UUID of the contract in KITOS</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{contractUuid}/handover")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItContractResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult PutItProjectHandoverData([NonEmptyGuid] Guid contractUuid, [FromBody] IEnumerable<HandoverTrialRequestDTO> request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates an existing it-contract's payment model data section
        /// </summary>
        /// <param name="contractUuid">UUID of the contract in KITOS</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{contractUuid}/payment-model")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItContractResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult PutItProjectPaymentModelData([NonEmptyGuid] Guid contractUuid, [FromBody] ContractPaymentModelDataWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates an existing it-contract's agreement period data section
        /// </summary>
        /// <param name="contractUuid">UUID of the contract in KITOS</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{contractUuid}/agreement-period")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItContractResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult PutItProjectAgreementPeriodData([NonEmptyGuid] Guid contractUuid, [FromBody] ContractAgreementPeriodDataWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates an existing it-contract's Termination section
        /// </summary>
        /// <param name="contractUuid">UUID of the contract in KITOS</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{contractUuid}/termination")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItContractResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult PutItProjectTerminationData([NonEmptyGuid] Guid contractUuid, [FromBody] ContractTerminationDataWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates an existing it-contract's payments section
        /// </summary>
        /// <param name="contractUuid">UUID of the contract in KITOS</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{contractUuid}/payments")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItContractResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult PutItProjectPaymentData([NonEmptyGuid] Guid contractUuid, [FromBody] ContractPaymentsDataWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates an existing it-contract's payments section
        /// </summary>
        /// <param name="contractUuid">UUID of the contract in KITOS</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{contractUuid}/roles")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItContractResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult PutItProjectRoleAssignmentData([NonEmptyGuid] Guid contractUuid, [FromBody] IEnumerable<RoleAssignmentRequestDTO> request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates an existing it-contract's references section
        /// </summary>
        /// <param name="contractUuid">UUID of the contract in KITOS</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{contractUuid}/external-references")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItContractResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult PutItProjectExternalReferencesData([NonEmptyGuid] Guid contractUuid, [FromBody] IEnumerable<ExternalReferenceDataDTO> request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new NotImplementedException();
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
        public IHttpActionResult DeleteItProject([NonEmptyGuid] Guid contractUuid, [FromBody] ContractWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new NotImplementedException();
        }

        private static ItContractResponseDTO ToItContractResponseDto(ItContract contract)
        {
            //TODO: To response mapper
            return new()
            {
                Uuid = contract.Uuid,
                Name = contract.Name,
            };
        }
    }
}