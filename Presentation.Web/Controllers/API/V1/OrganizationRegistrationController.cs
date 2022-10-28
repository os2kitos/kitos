using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.Organizations;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;
using Presentation.Web.Controllers.API.V1.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.Organizations;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V1
{
    [PublicApi]
    [RoutePrefix("api/v1/organization-registrations")]

    public class OrganizationRegistrationController: BaseApiController
    {
        private readonly IOrganizationUnitService _organizationUnitService;

        public OrganizationRegistrationController(IOrganizationUnitService organizationUnitService)
        {
            _organizationUnitService = organizationUnitService;
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
            return _organizationUnitService.DeleteAllUnitOrganizationRegistrations(organizationId, unitId)
                .Match(FromOperationError, Ok);
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

        private static OrganizationRegistrationDTO ToRegistrationDto(OrganizationRegistrationDetails details)
        {
            return new OrganizationRegistrationDTO
            {
                OrganizationUnitRights = details.OrganizationUnitRights.Select(MapUnitRightToNamedEntityDto).ToList(),
                ItContractRegistrations = details.ItContractRegistrations.Select(x => x.MapToNamedEntityDTO()).ToList(),
                Payments = details.PaymentRegistrationDetails.Select(ToPaymentRegistrationDto).ToList(),
                RelevantSystems = details.RelevantSystems.Select(x => x.MapToNamedEntityWithEnabledStatusDTO()).ToList(),
                ResponsibleSystems = details.ResponsibleSystems.Select(x => x.MapToNamedEntityWithEnabledStatusDTO()).ToList()
            };
        }

        private static PaymentRegistrationDTO ToPaymentRegistrationDto(PaymentRegistrationDetails details)
        {
            return new PaymentRegistrationDTO()
            {
                ItContract = details.ItContract.MapToNamedEntityDTO(),
                InternalPayments = details.InternalPayments.Select(MapPaymentToNamedEntityDto).ToList(),
                ExternalPayments = details.ExternalPayments.Select(MapPaymentToNamedEntityDto).ToList()
            };
        }

        private static NamedEntityDTO MapUnitRightToNamedEntityDto(OrganizationUnitRight right)
        {
            return new NamedEntityDTO
            {
                Id = right.Id,
                Name = right.Role.Name
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
    }
}