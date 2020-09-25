using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.GDPR;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.LocalOptions;
using Core.DomainModel.Shared;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Infrastructure.Services.Types;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Presentation.Web.Models.GDPR;
using Presentation.Web.Models.References;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [RoutePrefix("api/v1/data-processing-registration")]
    public class DataProcessingRegistrationController : BaseApiController
    {
        private readonly IDataProcessingRegistrationApplicationService _dataProcessingRegistrationApplicationService;
        private readonly IGenericRepository<LocalDataProcessingRegistrationRole> _localRoleRepository;

        public DataProcessingRegistrationController(
            IDataProcessingRegistrationApplicationService dataProcessingRegistrationApplicationService,
            IGenericRepository<LocalDataProcessingRegistrationRole> localRoleRepository)
        {
            _dataProcessingRegistrationApplicationService = dataProcessingRegistrationApplicationService;
            _localRoleRepository = localRoleRepository;
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
                .Match(systems => Ok(systems.MapToNamedEntityDTOs().ToList()), FromOperationError);
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

        [HttpPatch]
        [Route("{id}/agreement-concluded")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage PatchIsAgreementConcluded(int id, [FromBody] SingleValueDTO<YesNoIrrelevantOption> yesNoIrrelevantOption)
        {
            if (yesNoIrrelevantOption == null)
                return BadRequest("yesNoIrrelevantOption must be provided");

            return _dataProcessingRegistrationApplicationService
                .UpdateIsAgreementConcluded(id, yesNoIrrelevantOption.Value)
                .Match(_ => Ok(), FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/agreement-concluded-at")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage PatchAgreementConcludedAt(int id, [FromBody] SingleValueDTO<DateTime?> dateTime)
        {
            if (dateTime == null)
                return BadRequest("dateTime must be provided");

            return _dataProcessingRegistrationApplicationService
                .UpdateAgreementConcludedAt(id, dateTime.Value)
                .Match(_ => Ok(), FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/oversight-option")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage PatchOversightOption(int id, [FromBody] SingleValueDTO<YearMonthIntervalOption> oversightInterval)
        {
            if (oversightInterval == null)
                return BadRequest("YearMonthIntervalOption must provided");
            

            return _dataProcessingRegistrationApplicationService
                .UpdateOversightInterval(id, oversightInterval.Value)
                .Match(_ => Ok(), FromOperationError);
        }

        [HttpPatch]
        [Route("{id}/oversight-option-note")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage PatchOversightOptionNote(int id, [FromBody] SingleValueDTO<string> oversightIntervalNote)
        {
            if (oversightIntervalNote == null)
                return BadRequest("Note must be provided");

            return _dataProcessingRegistrationApplicationService
                .UpdateOversightIntervalNote(id, oversightIntervalNote.Value)
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
            var localDescriptionOverrides = GetLocalDescriptionOverrides(organizationId);

            return roles.Select(role => ToDTO(role, localDescriptionOverrides));
        }

        private List<DataProcessingRegistrationDTO> ToDTOs(IQueryable<DataProcessingRegistration> value, int organizationId)
        {
            var localDescriptionOverrides = GetLocalDescriptionOverrides(organizationId);

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
                .AsNoTracking()
                .AsEnumerable()
                .Select(dataProcessingRegistration => ToDTO(dataProcessingRegistration, localDescriptionOverrides))
                .ToList();
        }

        private Dictionary<int, Maybe<string>> GetLocalDescriptionOverrides(int organizationId)
        {
            var localDescriptionOverrides = _localRoleRepository
                .AsQueryable()
                .ByOrganizationId(organizationId)
                .ToDictionary(localDataProcessingRegistrationRole => localDataProcessingRegistrationRole.OptionId,
                    localDataProcessingRegistrationRole => string.IsNullOrWhiteSpace(localDataProcessingRegistrationRole.Description) ? Maybe<string>.None : localDataProcessingRegistrationRole.Description);
            return localDescriptionOverrides;
        }

        private DataProcessingRegistrationDTO ToDTO(DataProcessingRegistration value)
        {
            return ToDTO(value, GetLocalDescriptionOverrides(value.OrganizationId));
        }

        private static DataProcessingRegistrationDTO ToDTO(DataProcessingRegistration value, Dictionary<int, Maybe<string>> localDescriptionOverrides)
        {
            return new DataProcessingRegistrationDTO(value.Id, value.Name)
            {
                AssignedRoles = value.Rights.Select(dataProcessingRegistrationRight => new AssignedRoleDTO
                {
                    Role = ToDTO(dataProcessingRegistrationRight.Role, localDescriptionOverrides),
                    User = ToDTO(dataProcessingRegistrationRight.User)

                }).ToArray(),
                References = value
                    .ExternalReferences
                    .Select(externalReference => ToDTO(value.ReferenceId, externalReference))
                    .ToArray(),
                ItSystems = value
                    .GetAssignedSystems()
                    .Select(system => system.MapToNamedEntityWithEnabledStatusDTO())
                    .ToArray(),
                OversightInterval = new ValueOptionWithOptionalNoteDTO<YearMonthIntervalOption?>()
                {
                    Value = value.OversightInterval,
                    Note = value.OversightIntervalNote
                },
                DataProcessors = value
                    .DataProcessors
                    .Select(x => x.MapToShallowOrganizationDTO())
                    .ToArray(),
                AgreementConcluded = new Models.Shared.ValueOptionWithOptionalDateDTO<YesNoIrrelevantOption?>
                {
                    Value = value.IsAgreementConcluded,
                    OptionalDateValue = value.AgreementConcludedAt
                }
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

        private static BusinessRoleDTO ToDTO(DataProcessingRegistrationRole role, IReadOnlyDictionary<int, Maybe<string>> localDescriptionOverrides)
        {
            return new BusinessRoleDTO(role.Id, role.Name)
            {
                HasWriteAccess = role.HasWriteAccess,
                Note = localDescriptionOverrides.ContainsKey(role.Id)
                    ? localDescriptionOverrides[role.Id].GetValueOrFallback(role.Description)
                    : role.Description
            };
        }
    }
}