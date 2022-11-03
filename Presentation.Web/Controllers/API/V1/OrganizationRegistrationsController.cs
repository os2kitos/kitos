using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.Organizations;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Presentation.Web.Controllers.API.V1.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.Organizations;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V1
{
    [PublicApi]
    [RoutePrefix("api/v1/organization-units")]
    public class OrganizationRegistrationsController: BaseApiController
    {
        private readonly IOrganizationUnitService _organizationUnitService;
        private readonly IOrgUnitService _orgUnitService;

        public OrganizationRegistrationsController(IOrganizationUnitService organizationUnitService,
            IOrgUnitService orgUnitService)
        {
            _organizationUnitService = organizationUnitService;
            _orgUnitService = orgUnitService;
        }

        [HttpGet]
        [Route("{organizationId}/{unitId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetUnitRegistrations(int organizationId, int unitId)
        {
            return _organizationUnitService.GetOrganizationRegistrations(organizationId, unitId)
                .Select(ToRegistrationDto)
                .Match(Ok, FromOperationError);
        }

        [HttpDelete]
        [Route("{organizationId}/{unitId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage RemoveSelectedUnitRegistrations(int organizationId, int unitId, [FromBody] ChangeOrganizationRegistrationRequestDTO requestDto)
        {
            var changeParameters = ToChangeParameters(requestDto);
            return _organizationUnitService.DeleteSelectedOrganizationRegistrations(organizationId, unitId, changeParameters)
                .Match(FromOperationError, Ok);
        }

        [HttpDelete]
        [Route("unit/{organizationId}/{unitId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage DeleteUnitWithRegistrations(int organizationId, int unitId)
        {
            _orgUnitService.Delete(organizationId, unitId);
            return Ok();
        }

        [HttpPut]
        [Route("{organizationId}/{unitId}/{targetUnitId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage TransferSelectedUnitRegistrations(int organizationId, int unitId, int targetUnitId, [FromBody] ChangeOrganizationRegistrationRequestDTO requestDto)
        {
            var changeParameters = ToChangeParameters(requestDto);
            return _organizationUnitService.TransferSelectedOrganizationRegistrations(organizationId, unitId, targetUnitId, changeParameters)
                .Match(FromOperationError, Ok);
        }

        [HttpGet]
        [Route("access-rights/{organizationId}/{unitId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetUnitAccessRights(int organizationId, int unitId)
        {
            return _organizationUnitService.GetUnitAccessRightsByUnitId(organizationId, unitId)
                .Select(ToAccessRightsDto)
                .Match(Ok, FromOperationError);
        }

        private static OrganizationRegistrationDTO ToRegistrationDto(OrganizationRegistrationDetails details)
        {
            return new OrganizationRegistrationDTO
            {
                OrganizationUnitRights = details.OrganizationUnitRights.Select(MapUnitRightToNamedEntityDtoWithUserFullNameDto).ToList(),
                ItContractRegistrations = details.ItContractRegistrations.Select(x => x.MapToNamedEntityDTO()).ToList(),
                Payments = details.PaymentRegistrationDetails.Select(ToPaymentRegistrationDto).ToList(),
                RelevantSystems = details.RelevantSystems.Select(x => x.MapToNamedEntityWithEnabledStatusDTO()).ToList(),
                ResponsibleSystems = details.ResponsibleSystems.Select(x => x.MapToNamedEntityWithEnabledStatusDTO()).ToList()
            };
        }

        private static PaymentRegistrationDTO ToPaymentRegistrationDto(PaymentRegistrationDetails details)
        {
            return new PaymentRegistrationDTO
            {
                ItContract = details.ItContract.MapToNamedEntityDTO(),
                InternalPayments = details.InternalPayments.Select(MapPaymentToNamedEntityDto).ToList(),
                ExternalPayments = details.ExternalPayments.Select(MapPaymentToNamedEntityDto).ToList()
            };
        }

        private static NamedEntityWithUserFullNameDTO MapUnitRightToNamedEntityDtoWithUserFullNameDto(OrganizationUnitRight right)
        {
            return new NamedEntityWithUserFullNameDTO
            {
                Id = right.Id,
                Name = right.Role.Name,
                UserFullName = right.User.GetFullName()
            };
        }

        private static NamedEntityDTO MapPaymentToNamedEntityDto(EconomyStream payment)
        {
            return new NamedEntityDTO
            {
                Id = payment.Id,
                Name = $"{payment.Acquisition}, {payment.Operation}, {payment.Other}"
            };
        }

        private static OrganizationRegistrationChangeParameters ToChangeParameters(
            ChangeOrganizationRegistrationRequestDTO requestDto)
        {
            return new OrganizationRegistrationChangeParameters
            (
                requestDto.OrganizationUnitRights,
                requestDto.ItContractRegistrations,
                requestDto.PaymentRegistrationDetails.Select(x =>
                    new PaymentChangeParameters(x.ItContractId, x.InternalPayments, x.ExternalPayments)),
                requestDto.RelevantSystems,
                requestDto.ResponsibleSystems
            );
        }

        private static UnitAccessRightsDTO ToAccessRightsDto(
            UnitAccessRights accessRights)
        {
            return new UnitAccessRightsDTO(accessRights.CanBeRead, accessRights.CanBeModified, accessRights.CanNameBeModified, accessRights.CanBeRearranged, accessRights.CanBeDeleted);
        }
    }
}