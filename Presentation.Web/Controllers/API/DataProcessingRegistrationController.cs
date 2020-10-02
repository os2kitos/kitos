using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.GDPR;
using Core.ApplicationServices.Model.GDPR;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Core.DomainModel.Shared;
using Core.DomainServices.Model.Options;
using Infrastructure.Services.Types;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Presentation.Web.Models.GDPR;
using Presentation.Web.Models.References;
using Presentation.Web.Models.Shared;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [RoutePrefix("api/v1/data-processing-registration")]
    public class DataProcessingRegistrationController : BaseApiController
    {
        private readonly IDataProcessingRegistrationApplicationService _dataProcessingRegistrationApplicationService;
        private readonly IDataProcessingRegistrationOptionsApplicationService _dataProcessingRegistrationOptionsApplicationService;

        public DataProcessingRegistrationController(
            IDataProcessingRegistrationApplicationService dataProcessingRegistrationApplicationService,
            IDataProcessingRegistrationOptionsApplicationService dataProcessingRegistrationOptionsApplicationService)
        {
            _dataProcessingRegistrationApplicationService = dataProcessingRegistrationApplicationService;
            _dataProcessingRegistrationOptionsApplicationService = dataProcessingRegistrationOptionsApplicationService;
        }

        protected override IEntity GetEntity(int id) => _dataProcessingRegistrationApplicationService.Get(id).Match(dataProcessingRegistration => dataProcessingRegistration, _ => null);

        protected override bool AllowCreateNewEntity(int organizationId) => AllowCreate<DataProcessingRegistration>(organizationId);

        [HttpGet]
        [Route]
        [InternalApi]
        public override HttpResponseMessage GetAccessRights(bool? getEntitiesAccessRights, int organizationId) => base.GetAccessRights(getEntitiesAccessRights, organizationId);

        [HttpGet]
        [Route]
        [InternalApi]
        public override HttpResponseMessage GetAccessRightsForEntity(int id, bool? getEntityAccessRights) => base.GetAccessRightsForEntity(id, getEntityAccessRights);

        [HttpPost]
        [Route]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(ApiReturnDTO<DataProcessingRegistrationDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public HttpResponseMessage Post([FromBody] CreateDataProcessingRegistrationDTO dto)
        {
            if (dto == null)
                return BadRequest("No input parameters provided");

            return _dataProcessingRegistrationApplicationService
                .Create(dto.OrganizationId, dto.Name)
                .Match(value => Created(ToDTO(value), new Uri(Request.RequestUri + "/" + value.Id)), FromOperationError);
        }

        [HttpGet]
        [Route("{id}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<DataProcessingRegistrationDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage Get(int id)
        {
            return _dataProcessingRegistrationApplicationService
                .Get(id)
                .Match(value => Ok(ToDTO(value)), FromOperationError);
        }

        [HttpGet]
        [Route("defined-in/{organizationId}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<DataProcessingRegistrationDTO[]>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public HttpResponseMessage GetOrganizationData(int organizationId, int skip, int take)
        {
            return _dataProcessingRegistrationApplicationService
                .GetOrganizationData(organizationId, skip, take)
                .Match(value => Ok(ToDTOs(value, organizationId)), FromOperationError);
        }

        [HttpDelete]
        [Route("{id}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage Delete(int id)
        {
            return _dataProcessingRegistrationApplicationService
                .Delete(id)
                .Match(value => Ok(), FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/name")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public HttpResponseMessage ChangeName(int id, [FromBody] SingleValueDTO<string> value)
        {
            return _dataProcessingRegistrationApplicationService
                .UpdateName(id, value.Value)
                .Match(_ => Ok(), FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/master-reference")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage SetMasterReference(int id, [FromBody] SingleValueDTO<int> value)
        {
            return _dataProcessingRegistrationApplicationService
                .SetMasterReference(id, value.Value)
                .Match(_ => Ok(), FromOperationError);
        }

        /// <summary>
        /// Use internally to query whether a new agreement can be created with the suggested parameters
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet]
        [InternalApi]
        [Route]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public HttpResponseMessage CanCreate(int orgId, string checkname)
        {
            return _dataProcessingRegistrationApplicationService
                .ValidateSuggestedNewRegistrationName(orgId, checkname)
                .Select(FromOperationError)
                .GetValueOrFallback(Ok());
        }

        [HttpGet]
        [Route("{id}/available-roles")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<BusinessRoleDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [InternalApi]
        public HttpResponseMessage GetAvailableRoles(int id)
        {
            return _dataProcessingRegistrationApplicationService
                .GetAvailableRoles(id)
                .Select<IEnumerable<BusinessRoleDTO>>(result => ToDTOs(result.roles, result.registration.OrganizationId).ToList())
                .Match(Ok, FromOperationError);

        }

        [HttpGet]
        [Route("{id}/available-roles/{roleId}/applicable-users")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<UserWithEmailDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [InternalApi]
        public HttpResponseMessage GetApplicableUsers(int id, int roleId, [FromUri] string nameOrEmailContent = null, [FromUri] int pageSize = 25)
        {
            return _dataProcessingRegistrationApplicationService
                .GetUsersWhichCanBeAssignedToRole(id, roleId, nameOrEmailContent, pageSize)
                .Select<IEnumerable<UserWithEmailDTO>>(users => ToDTOs(users).ToList())
                .Match(Ok, FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/roles/assign")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public HttpResponseMessage AssignNewRole(int id, [FromBody] AssignRoleDTO dto)
        {
            if (dto == null)
                return BadRequest("No input parameters provided");

            return
                _dataProcessingRegistrationApplicationService
                    .AssignRole(id, dto.RoleId, dto.UserId)
                    .Match(_ => Ok(), FromOperationError);

        }

        [HttpPatch]
        [Route("{id}/roles/remove/{roleId}/from/{userId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public HttpResponseMessage RemoveRole(int id, int roleId, int userId)
        {
            return
                _dataProcessingRegistrationApplicationService
                    .RemoveRole(id, roleId, userId)
                    .Match(_ => Ok(), FromOperationError);
        }

        [HttpGet]
        [Route("{id}/it-systems/available")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetAvailableSystems(int id, [FromUri] string nameQuery = null, [FromUri] int pageSize = 25)
        {
            return _dataProcessingRegistrationApplicationService
                .GetSystemsWhichCanBeAssigned(id, nameQuery, pageSize)
                .Match(systems => Ok(systems.Select(x => x.MapToNamedEntityWithEnabledStatusDTO()).ToList()), FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/it-systems/assign")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public HttpResponseMessage AssignSystem(int id, [FromBody] SingleValueDTO<int> systemId)
        {
            if (systemId == null)
                return BadRequest("systemId must be provided");

            return _dataProcessingRegistrationApplicationService
                .AssignSystem(id, systemId.Value)
                .Match(_ => Ok(), FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/it-systems/remove")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage RemoveSystem(int id, [FromBody] SingleValueDTO<int> systemId)
        {
            if (systemId == null)
                return BadRequest("systemId must be provided");

            return _dataProcessingRegistrationApplicationService
                .RemoveSystem(id, systemId.Value)
                .Match(_ => Ok(), FromOperationError);
        }

        [HttpGet]
        [Route("{id}/data-processors/available")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetAvailableDataProcessors(int id, [FromUri] string nameQuery = null, [FromUri] int pageSize = 25)
        {
            return _dataProcessingRegistrationApplicationService
                .GetDataProcessorsWhichCanBeAssigned(id, nameQuery, pageSize)
                .Match(organizations => Ok(organizations.Select(x => x.MapToShallowOrganizationDTO()).ToList()), FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/data-processors/assign")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public HttpResponseMessage AssignDataProcessor(int id, [FromBody] SingleValueDTO<int> organizationId)
        {
            if (organizationId == null)
                return BadRequest("organizationId must be provided");

            return _dataProcessingRegistrationApplicationService
                .AssignDataProcessor(id, organizationId.Value)
                .Match(_ => Ok(), FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/data-processors/remove")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage RemoveDataProcessor(int id, [FromBody] SingleValueDTO<int> organizationId)
        {
            if (organizationId == null)
                return BadRequest("organizationId must be provided");

            return _dataProcessingRegistrationApplicationService
                .RemoveDataProcessor(id, organizationId.Value)
                .Match(_ => Ok(), FromOperationError);
        }

        [HttpGet]
        [Route("{id}/sub-data-processors/available")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetAvailableSubDataProcessors(int id, [FromUri] string nameQuery = null, [FromUri] int pageSize = 25)
        {
            return _dataProcessingRegistrationApplicationService
                .GetSubDataProcessorsWhichCanBeAssigned(id, nameQuery, pageSize)
                .Match(organizations => Ok(organizations.Select(x => x.MapToShallowOrganizationDTO()).ToList()), FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/sub-data-processors/state")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage SetSubDataProcessorsState(int id, [FromBody] SingleValueDTO<YesNoUndecidedOption> value)
        {
            if (value == null)
                return BadRequest("value must be provided");

            return _dataProcessingRegistrationApplicationService
                .SetSubDataProcessorsState(id, value.Value)
                .Match(dataProcessingRegistration => Ok(ToDTO(dataProcessingRegistration)), FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/sub-data-processors/assign")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public HttpResponseMessage AssignSubDataProcessor(int id, [FromBody] SingleValueDTO<int> organizationId)
        {
            if (organizationId == null)
                return BadRequest("organizationId must be provided");

            return _dataProcessingRegistrationApplicationService
                .AssignSubDataProcessor(id, organizationId.Value)
                .Match(_ => Ok(), FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/sub-data-processors/remove")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage RemoveSubDataProcessor(int id, [FromBody] SingleValueDTO<int> organizationId)
        {
            if (organizationId == null)
                return BadRequest("organizationId must be provided");

            return _dataProcessingRegistrationApplicationService
                .RemoveSubDataProcessor(id, organizationId.Value)
                .Match(_ => Ok(), FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/agreement-concluded")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage PatchIsAgreementConcluded(int id, [FromBody] SingleValueDTO<YesNoIrrelevantOption> concluded)
        {
            if (concluded == null)
                return BadRequest("concluded must be provided");

            return _dataProcessingRegistrationApplicationService
                .UpdateIsAgreementConcluded(id, concluded.Value)
                .Match(dataProcessingRegistration => Ok(ToDTO(dataProcessingRegistration)), FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/agreement-concluded-at")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage PatchAgreementConcludedAt(int id, [FromBody] SingleValueDTO<DateTime?> concludedAt)
        {
            if (concludedAt == null)
                return BadRequest("concludedAt must be provided");

            return _dataProcessingRegistrationApplicationService
                .UpdateAgreementConcludedAt(id, concludedAt.Value)
                .Match(_ => Ok(), FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/insecure-third-countries/state")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage SetTransferToInsecureCountriesState(int id, [FromBody] SingleValueDTO<YesNoUndecidedOption> value)
        {
            if (value == null)
                return BadRequest("value must be provided");

            return _dataProcessingRegistrationApplicationService
                .UpdateTransferToInsecureThirdCountries(id, value.Value)
                .Match(dataProcessingRegistration => Ok(ToDTO(dataProcessingRegistration)), FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/insecure-third-countries/assign")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public HttpResponseMessage AssignInsecureThirdCountry(int id, [FromBody] SingleValueDTO<int> countryId)
        {
            if (countryId == null)
                return BadRequest($"{nameof(countryId)} must be provided");

            return _dataProcessingRegistrationApplicationService
                .AssignInsecureThirdCountry(id, countryId.Value)
                .Match(_ => Ok(), FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/insecure-third-countries/remove")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage RemoveInsecureThirdCountry(int id, [FromBody] SingleValueDTO<int> countryId)
        {
            if (countryId == null)
                return BadRequest($"{nameof(countryId)} must be provided");

            return _dataProcessingRegistrationApplicationService
                .RemoveInsecureThirdCountry(id, countryId.Value)
                .Match(_ => Ok(), FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/basis-for-transfer/assign")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage AssignbBasisForTransfer(int id, [FromBody] SingleValueDTO<int> basisForTransferId)
        {
            if (basisForTransferId == null)
                return BadRequest($"{nameof(basisForTransferId)} must be provided");

            return _dataProcessingRegistrationApplicationService
                .AssignBasisForTransfer(id, basisForTransferId.Value)
                .Match(_ => Ok(), FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/basis-for-transfer/clear")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage ClearBasisForTransfer(int id)
        {
            return _dataProcessingRegistrationApplicationService
                .ClearBasisForTransfer(id)
                .Match(_ => Ok(), FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/oversight-interval")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage PatchOversightOption(int id, [FromBody] SingleValueDTO<YearMonthIntervalOption> oversightInterval)
        {
            if (oversightInterval == null)
                return BadRequest(nameof(oversightInterval) + " must provided");


            return _dataProcessingRegistrationApplicationService
                .UpdateOversightInterval(id, oversightInterval.Value)
                .Match(_ => Ok(), FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/oversight-interval-remark")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage PatchOversightIntervalRemark(int id, [FromBody] SingleValueDTO<string> oversightIntervalRemark)
        {
            if (oversightIntervalRemark == null)
                return BadRequest(nameof(oversightIntervalRemark) + " must be provided");

            return _dataProcessingRegistrationApplicationService
                .UpdateOversightIntervalRemark(id, oversightIntervalRemark.Value)
                .Match(_ => Ok(), FromOperationError);
        }

        [HttpGet]
        [Route("available-options-in/{organizationId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetDataProcessingRegistrationOptions(int organizationId)
        {
            return _dataProcessingRegistrationOptionsApplicationService
                .GetAssignableDataProcessingRegistrationOptions(organizationId)
                .Select<DataProcessingOptionsDTO>(result => new DataProcessingOptionsDTO
                {
                    DataResponsibleOptions = ToDTOs(result.DataResponsibleOptions, organizationId).ToList(),
                    ThirdCountryOptions = ToDTOs(result.ThirdCountryOptions, organizationId).ToList(),
                    BasisForTransferOptions = ToDTOs(result.BasisForTransferOptions, organizationId).ToList(),
                    Roles = result.Roles.Select(ToDto).ToList(),
                    OversightOptions = ToDTOs(result.OversightOptions, organizationId).ToList()
                })
                .Match(Ok, FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/data-responsible/assign")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage AssignDataResponsible(int id, [FromBody] SingleValueDTO<int> dataResponsibleId)
        {
            if (dataResponsibleId == null)
                return BadRequest($"{nameof(dataResponsibleId)} must be provided");

            return _dataProcessingRegistrationApplicationService
                .AssignDataResponsible(id, dataResponsibleId.Value)
                .Match(_ => Ok(), FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/data-responsible/clear")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage ClearDataResponsible(int id)
        {
            return _dataProcessingRegistrationApplicationService
                .ClearDataResponsible(id)
                .Match(_ => Ok(), FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/data-responsible-remark")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage PatchDataResponsibleRemark(int id, [FromBody] SingleValueDTO<string> remark)
        {
            if (remark == null)
                return BadRequest($"{nameof(remark)} must be provided");

            return _dataProcessingRegistrationApplicationService
                .UpdateDataResponsibleRemark(id, remark.Value)
                .Match(_ => Ok(), FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/oversight-option/assign")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public HttpResponseMessage AssignOversightOption(int id, [FromBody] SingleValueDTO<int> oversightOptionId)
        {
            if (oversightOptionId == null)
                return BadRequest($"{nameof(oversightOptionId)} must be provided");

            return _dataProcessingRegistrationApplicationService
                .AssignOversightOption(id, oversightOptionId.Value)
                .Match(_ => Ok(), FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/oversight-option/remove")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage RemoveOversightOption(int id, [FromBody] SingleValueDTO<int> oversightOptionId)
        {
            if (oversightOptionId == null)
                return BadRequest($"{nameof(oversightOptionId)} must be provided");

            return _dataProcessingRegistrationApplicationService
                .RemoveOversightOption(id, oversightOptionId.Value)
                .Match(_ => Ok(), FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/oversight-option-remark")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage PatchOversightOptionRemark(int id, [FromBody] SingleValueDTO<string> remark)
        {
            if (remark == null)
                return BadRequest($"{nameof(remark)} must be provided");

            return _dataProcessingRegistrationApplicationService
                .UpdateOversightOptionRemark(id, remark.Value)
                .Match(_ => Ok(), FromOperationError);
        }

        private static IEnumerable<UserWithEmailDTO> ToDTOs(IEnumerable<User> users)
        {
            return users.Select(ToDTO);
        }

        private static UserWithEmailDTO ToDTO(User arg)
        {
            return new UserWithEmailDTO(arg.Id, $"{arg.Name} {arg.LastName}", arg.Email);
        }

        private IEnumerable<BusinessRoleDTO> ToDTOs(IEnumerable<DataProcessingRegistrationRole> roles, int organizationId)
        {
            var dataProcessingRegistrationOptions = _dataProcessingRegistrationOptionsApplicationService.GetAssignableDataProcessingRegistrationOptions(organizationId).Value;
            var localDescriptionOverrides = GetLocalRoleDescriptionOverrides(dataProcessingRegistrationOptions);

            var enabledRoles = GetIdsOfAvailableRoles(dataProcessingRegistrationOptions);

            return roles.Select(role => ToDTO(role, localDescriptionOverrides, enabledRoles));
        }

        private List<DataProcessingRegistrationDTO> ToDTOs(IQueryable<DataProcessingRegistration> value, int organizationId)
        {
            var localDescriptionOverrides = GetLocalRoleDescriptionOverrides(_dataProcessingRegistrationOptionsApplicationService.GetAssignableDataProcessingRegistrationOptions(organizationId));
            var assignableDataProcessingRegistrationOptions = _dataProcessingRegistrationOptionsApplicationService.GetAssignableDataProcessingRegistrationOptions(organizationId).Value;
            var enabledCountryOptions = GetIdsOfAvailableCountryOptions(assignableDataProcessingRegistrationOptions);
            var enabledBasisForTransferOptions = GetIdsOfAvailableBasisForTransferOptions(assignableDataProcessingRegistrationOptions);
            var enabledDataResponsibleOptions = GetIdsOfAvailableDataResponsibleOptions(assignableDataProcessingRegistrationOptions);
            var enabledRoles = GetIdsOfAvailableRoles(assignableDataProcessingRegistrationOptions);
            var enabledOversightOptions = GetIdsOfAvailableOversightOptions(assignableDataProcessingRegistrationOptions);

            return value
                .Include(dataProcessingRegistration => dataProcessingRegistration.Rights)
                .Include(dataProcessingRegistration => dataProcessingRegistration.ExternalReferences)
                .Include(dataProcessingRegistration => dataProcessingRegistration.Reference)
                .Include(dataProcessingRegistration => dataProcessingRegistration.Reference.ObjectOwner)
                .Include(dataProcessingRegistration => dataProcessingRegistration.Rights.Select(_ => _.Role))
                .Include(dataProcessingRegistration => dataProcessingRegistration.Rights.Select(_ => _.User))
                .Include(dataProcessingRegistration => dataProcessingRegistration.SystemUsages)
                .Include(dataProcessingRegistration => dataProcessingRegistration.SystemUsages.Select(x => x.ItSystem))
                .Include(dataProcessingRegistration => dataProcessingRegistration.DataProcessors)
                .Include(dataProcessingRegistration => dataProcessingRegistration.SubDataProcessors)
                .Include(dataProcessingRegistration => dataProcessingRegistration.InsecureCountriesSubjectToDataTransfer)
                .Include(dataProcessingRegistration => dataProcessingRegistration.BasisForTransfer)
                .AsEnumerable()
                .Select(dataProcessingRegistration => ToDTO(dataProcessingRegistration, localDescriptionOverrides, enabledCountryOptions, enabledBasisForTransferOptions, enabledDataResponsibleOptions, enabledRoles, enabledOversightOptions))
                .ToList();
        }

        private ISet<int> GetIdsOfAvailableCountryOptions(DataProcessingRegistrationOptions dataProcessingRegistrationOptions)
        {
            return new HashSet<int>(dataProcessingRegistrationOptions.ThirdCountryOptions.Select(x => x.Option.Id));
        }

        private ISet<int> GetIdsOfAvailableRoles(DataProcessingRegistrationOptions dataProcessingRegistrationOptions)
        {
            return new HashSet<int>(dataProcessingRegistrationOptions.Roles.Select(x => x.Option.Id));
        }

        private ISet<int> GetIdsOfAvailableDataResponsibleOptions(DataProcessingRegistrationOptions dataProcessingRegistrationOptions)
        {
            return new HashSet<int>(dataProcessingRegistrationOptions.DataResponsibleOptions.Select(x => x.Option.Id));
        }

        private ISet<int> GetIdsOfAvailableBasisForTransferOptions(DataProcessingRegistrationOptions dataProcessingRegistrationOptions)
        {
            return new HashSet<int>(dataProcessingRegistrationOptions.BasisForTransferOptions.Select(x => x.Option.Id));
        }
        private ISet<int> GetIdsOfAvailableOversightOptions(DataProcessingRegistrationOptions dataProcessingRegistrationOptions)
        {
            return new HashSet<int>(dataProcessingRegistrationOptions.OversightOptions.Select(x => x.Option.Id));
        }

        private Dictionary<int, Maybe<string>> GetLocalRoleDescriptionOverrides(Result<DataProcessingRegistrationOptions, OperationError> options)
        {
            var localDescriptionOverrides = options
                .Value
                .Roles
                .ToDictionary(localDataProcessingRegistrationRole => localDataProcessingRegistrationRole.Option.Id,
                    localDataProcessingRegistrationRole => string.IsNullOrWhiteSpace(localDataProcessingRegistrationRole.Description) ? Maybe<string>.None : localDataProcessingRegistrationRole.Description);
            return localDescriptionOverrides;
        }

        private DataProcessingRegistrationDTO ToDTO(DataProcessingRegistration value)
        {
            var assignableDataProcessingRegistrationOptions = _dataProcessingRegistrationOptionsApplicationService.GetAssignableDataProcessingRegistrationOptions(value.OrganizationId).Value;
            var enabledCountryOptions = GetIdsOfAvailableCountryOptions(assignableDataProcessingRegistrationOptions);
            var enabledBasisForTransferOptions = GetIdsOfAvailableBasisForTransferOptions(assignableDataProcessingRegistrationOptions);
            var enabledDataResponsibleOptions = GetIdsOfAvailableDataResponsibleOptions(assignableDataProcessingRegistrationOptions);
            var enabledRoles = GetIdsOfAvailableRoles(assignableDataProcessingRegistrationOptions);
            var enabledOversightOptions = GetIdsOfAvailableOversightOptions(assignableDataProcessingRegistrationOptions);
            int organizationId = value.OrganizationId;
            return ToDTO(
                value, 
                GetLocalRoleDescriptionOverrides(_dataProcessingRegistrationOptionsApplicationService.GetAssignableDataProcessingRegistrationOptions(organizationId)), 
                enabledCountryOptions, 
                enabledBasisForTransferOptions, 
                enabledDataResponsibleOptions, 
                enabledRoles, 
                enabledOversightOptions);
        }

        private static DataProcessingRegistrationDTO ToDTO(
            DataProcessingRegistration value,
            Dictionary<int, Maybe<string>> localDescriptionOverrides,
            ISet<int> enabledCountryOptions,
            ISet<int> enabledBasisForTransferOptions,
            ISet<int> enabledDataResponsibleOptions,
            ISet<int> idsOfAvailableRoles,
            ISet<int> enabledOversightOptions)
        {
            return new DataProcessingRegistrationDTO(value.Id, value.Name)
            {
                AssignedRoles = value.Rights.Select(dataProcessingRegistrationRight => new AssignedRoleDTO
                {
                    Role = ToDTO(dataProcessingRegistrationRight.Role, localDescriptionOverrides, idsOfAvailableRoles),
                    User = ToDTO(dataProcessingRegistrationRight.User)

                }).ToArray(),
                References = value
                    .ExternalReferences
                    .Select(externalReference => ToDTO(value.ReferenceId, externalReference))
                    .ToArray(),
                ItSystems = value
                    .SystemUsages
                    .Select(system => system.MapToNamedEntityWithEnabledStatusDTO())
                    .ToArray(),
                OversightInterval = new ValueWithOptionalRemarkDTO<YearMonthIntervalOption?>()
                {
                    Value = value.OversightInterval,
                    Remark = value.OversightIntervalRemark
                },
                DataProcessors = value
                    .DataProcessors
                    .Select(x => x.MapToShallowOrganizationDTO())
                    .ToArray(),
                SubDataProcessors = value
                    .SubDataProcessors
                    .Select(x => x.MapToShallowOrganizationDTO())
                    .ToArray(),
                HasSubDataProcessors = value.HasSubDataProcessors,
                AgreementConcluded = new Models.Shared.ValueOptionWithOptionalDateDTO<YesNoIrrelevantOption?>
                {
                    Value = value.IsAgreementConcluded,
                    OptionalDateValue = value.AgreementConcludedAt
                },
                TransferToInsecureThirdCountries = value.TransferToInsecureThirdCountries,
                InsecureThirdCountries = value
                    .InsecureCountriesSubjectToDataTransfer
                    .Select(x => new NamedEntityWithExpirationStatusDTO(x.Id, x.Name, enabledCountryOptions.Contains(x.Id) == false))
                    .ToArray(),
                BasisForTransfer = value
                    .BasisForTransfer
                    .FromNullable()
                    .Select(basisForTransfer => new NamedEntityWithExpirationStatusDTO(basisForTransfer.Id, basisForTransfer.Name, enabledBasisForTransferOptions.Contains(basisForTransfer.Id) == false))
                    .GetValueOrDefault(),
                DataResponsible = new ValueWithOptionalRemarkDTO<OptionWithDescriptionAndExpirationDTO>()
                {
                    Value = value
                            .DataResponsible
                            .FromNullable()
                            .Select(responsible => new OptionWithDescriptionAndExpirationDTO(responsible.Id, responsible.Name, enabledDataResponsibleOptions.Contains(responsible.Id) == false, responsible.Description))
                            .GetValueOrDefault(),
                    Remark = value.DataResponsibleRemark
                },
                OversightOptions = new ValueWithOptionalRemarkDTO<NamedEntityWithExpirationStatusDTO[]>()
                {
                    Value = value
                            .OversightOptions
                            .Select(oversightOption => new NamedEntityWithExpirationStatusDTO(oversightOption.Id, oversightOption.Name, enabledOversightOptions.Contains(oversightOption.Id) == false))
                            .ToArray(),
                    Remark = value.OverSightOptionRemark
                },
            };
        }

        private static ReferenceDTO ToDTO(int? masterReferenceId, ExternalReference reference)
        {
            return new ReferenceDTO(reference.Id, reference.Title)
            {
                MasterReference = masterReferenceId.HasValue && masterReferenceId == reference.Id,
                ReferenceId = reference.ExternalReferenceId,
                Url = reference.URL,
                CreatedAt = reference.Created,
                CreatedByUser = reference.ObjectOwner.MapToNamedEntityDTO()
            };
        }

        private static BusinessRoleDTO ToDto(OptionDescriptor<DataProcessingRegistrationRole> availableRole)
        {
            return new BusinessRoleDTO(availableRole.Option.Id, availableRole.Option.Name, false, availableRole.Option.HasWriteAccess, availableRole.Description);
        }

        private static BusinessRoleDTO ToDTO(DataProcessingRegistrationRole role, IReadOnlyDictionary<int, Maybe<string>> localDescriptionOverrides, ISet<int> idsOfAvailableRoles)
        {
            return new BusinessRoleDTO(
                role.Id,
                role.Name,
                idsOfAvailableRoles.Contains(role.Id) == false,
                role.HasWriteAccess,
                localDescriptionOverrides.ContainsKey(role.Id)
                    ? localDescriptionOverrides[role.Id].Match(text => text, () => role.Description)
                    : role.Description);
        }

        private IEnumerable<OptionWithDescriptionDTO> ToDTOs<T>(IEnumerable<OptionDescriptor<T>> options, int organizationId) where T : OptionEntity<DataProcessingRegistration>
        {
            return options.Select(option => ToDTO(option));
        }

        private static OptionWithDescriptionDTO ToDTO<T>(OptionDescriptor<T> option) where T : OptionEntity<DataProcessingRegistration>
        {
            return new OptionWithDescriptionDTO(option.Option.Id, option.Option.Name, option.Description);
        }

    }
}