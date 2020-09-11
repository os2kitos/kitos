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
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Infrastructure.Services.Types;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Presentation.Web.Models.GDPR;
using Presentation.Web.Models.References;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [RoutePrefix("api/v1/data-processing-agreement")]
    public class DataProcessingAgreementController : BaseApiController
    {
        private readonly IDataProcessingAgreementApplicationService _dataProcessingAgreementApplicationService;
        private readonly IGenericRepository<LocalDataProcessingAgreementRole> _localRoleRepository;

        public DataProcessingAgreementController(
            IDataProcessingAgreementApplicationService dataProcessingAgreementApplicationService,
            IGenericRepository<LocalDataProcessingAgreementRole> localRoleRepository)
        {
            _dataProcessingAgreementApplicationService = dataProcessingAgreementApplicationService;
            _localRoleRepository = localRoleRepository;
        }

        protected override IEntity GetEntity(int id) => _dataProcessingAgreementApplicationService.Get(id).Match(agreement => agreement, _ => null);

        protected override bool AllowCreateNewEntity(int organizationId) => AllowCreate<DataProcessingAgreement>(organizationId);

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
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(ApiReturnDTO<DataProcessingAgreementDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public HttpResponseMessage Post([FromBody] CreateDataProcessingAgreementDTO dto)
        {
            if (dto == null)
                return BadRequest("No input parameters provided");

            return _dataProcessingAgreementApplicationService
                .Create(dto.OrganizationId, dto.Name)
                .Match(value => Created(ToDTO(value), new Uri(Request.RequestUri + "/" + value.Id)), FromOperationError);
        }

        [HttpGet]
        [Route("{id}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<DataProcessingAgreementDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage Get(int id)
        {
            return _dataProcessingAgreementApplicationService
                .Get(id)
                .Match(value => Ok(ToDTO(value)), FromOperationError);
        }

        [HttpGet]
        [Route("defined-in/{organizationId}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<DataProcessingAgreementDTO[]>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public HttpResponseMessage GetOrganizationData(int organizationId, int skip, int take)
        {
            return _dataProcessingAgreementApplicationService
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
            return _dataProcessingAgreementApplicationService
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
            return _dataProcessingAgreementApplicationService
                .UpdateName(id, value.Value)
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
            return _dataProcessingAgreementApplicationService
                .ValidateSuggestedNewAgreement(orgId, checkname)
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
            return _dataProcessingAgreementApplicationService
                .GetAvailableRoles(id)
                .Select<IEnumerable<BusinessRoleDTO>>(result => ToDTOs(result.roles, result.agreement.OrganizationId).ToList())
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
            return _dataProcessingAgreementApplicationService
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
                _dataProcessingAgreementApplicationService
                    .AssignRole(id, dto.RoleId, dto.UserId)
                    .Match(_ => Ok(), FromOperationError);

        }

        [HttpPatch]
        [Route("{id}/roles/remove/{roleId}/from/{userId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public HttpResponseMessage RemoveRole(int id, int roleId, int userId)
        {
            return
                _dataProcessingAgreementApplicationService
                    .RemoveRole(id, roleId, userId)
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

        private IEnumerable<BusinessRoleDTO> ToDTOs(IEnumerable<DataProcessingAgreementRole> roles, int organizationId)
        {
            var localDescriptionOverrides = GetLocalDescriptionOverrides(organizationId);

            return roles.Select(role => ToDTO(role, localDescriptionOverrides));
        }

        private List<DataProcessingAgreementDTO> ToDTOs(IQueryable<DataProcessingAgreement> value, int organizationId)
        {
            var localDescriptionOverrides = GetLocalDescriptionOverrides(organizationId);

            return value
                .Include(agreement => agreement.Rights)
                .Include(agreement => agreement.ExternalReferences)
                .Include(agreement => agreement.Reference)
                .Include(agreement => agreement.Rights.Select(_ => _.Role))
                .Include(agreement => agreement.Rights.Select(_ => _.User))
                .AsNoTracking()
                .AsEnumerable()
                .Select(agreement => ToDTO(agreement, localDescriptionOverrides))
                .ToList();
        }

        private Dictionary<int, Maybe<string>> GetLocalDescriptionOverrides(int organizationId)
        {
            var localDescriptionOverrides = _localRoleRepository
                .AsQueryable()
                .ByOrganizationId(organizationId)
                .ToDictionary(agreementRole => agreementRole.OptionId,
                    agreementRole => string.IsNullOrWhiteSpace(agreementRole.Description) ? Maybe<string>.None : agreementRole.Description);
            return localDescriptionOverrides;
        }

        private DataProcessingAgreementDTO ToDTO(DataProcessingAgreement value)
        {
            return ToDTO(value, GetLocalDescriptionOverrides(value.OrganizationId));
        }

        private static DataProcessingAgreementDTO ToDTO(DataProcessingAgreement value, Dictionary<int, Maybe<string>> localDescriptionOverrides)
        {
            return new DataProcessingAgreementDTO(value.Id, value.Name)
            {
                AssignedRoles = value.Rights.Select(agreementRight => new AssignedRoleDTO
                {
                    Role = ToDTO(agreementRight.Role, localDescriptionOverrides),
                    User = ToDTO(agreementRight.User)

                }).ToArray(),
                References = value.ExternalReferences.Select(externalReference => ToDTO(value.ReferenceId, externalReference)).ToArray()

            };
        }

        private static ReferenceDTO ToDTO(int? masterReferenceId, ExternalReference reference)
        {
            return new ReferenceDTO(reference.Id, reference.Title)
            {
                MasterReference = masterReferenceId.HasValue && masterReferenceId == reference.Id,
                ReferenceId = reference.ExternalReferenceId,
                Url = reference.URL
            };
        }

        private static BusinessRoleDTO ToDTO(DataProcessingAgreementRole role, IReadOnlyDictionary<int, Maybe<string>> localDescriptionOverrides)
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