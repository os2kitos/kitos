using System;
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
    [RoutePrefix("api/v1/organizations/{organizationUuid}/organization-units/{unitUuid}")]

    public class OrganizationRegistrationController: BaseApiController
    {
        private readonly IOrganizationUnitService _organizationUnitService;
        private readonly IOrgUnitService _orgUnitService;

        public OrganizationRegistrationController(IOrganizationUnitService organizationUnitService,
            IOrgUnitService orgUnitService)
        {
            _organizationUnitService = organizationUnitService;
            _orgUnitService = orgUnitService;
        }

        [HttpGet]
        [Route("registrations")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetRegistrations(Guid organizationUuid, Guid  unitUuid)
        {
            return _organizationUnitService.GetRegistrations(organizationUuid, unitUuid)
                .Select(ToRegistrationDto)
                .Match(Ok, FromOperationError);
        }

        [HttpDelete]
        [Route("registrations")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage RemoveRegistrations(Guid organizationUuid, Guid unitUuid, [FromBody] ChangeOrganizationRegistrationRequestDTO requestDto)
        {
            var changeParameters = ToChangeParameters(requestDto);
            return _organizationUnitService.DeleteRegistrations(organizationUuid, unitUuid, changeParameters)
                .Match(FromOperationError, Ok);
        }

        [HttpDelete]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage Delete(Guid organizationUuid, Guid unitUuid)
        {
            return _orgUnitService.Delete(organizationUuid, unitUuid)
                .Match(FromOperationError, Ok);
        }

        [HttpPut]
        [Route("registrations/{targetUnitUuid}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage TransferRegistrations(Guid organizationUuid, Guid unitUuid, Guid targetUnitUuid, [FromBody] ChangeOrganizationRegistrationRequestDTO requestDto)
        {
            var changeParameters = ToChangeParameters(requestDto);
            return _organizationUnitService.TransferRegistrations(organizationUuid, unitUuid, targetUnitUuid, changeParameters)
                .Match(FromOperationError, Ok);
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
                requestDto.ResponsibleSystems,
                requestDto.RelevantSystems
            );
        }
    }
}