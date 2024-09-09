using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.Organizations;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.Swagger.Annotations;
using System.Net;
using System;
using System.Linq;
using System.Web.Http;
using Presentation.Web.Models.API.V2.Internal.Common;
using Presentation.Web.Models.API.V2.Internal.Request.OrganizationUnit;
using Presentation.Web.Models.API.V2.Internal.Response.OrganizationUnit;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Core.DomainModel.ItSystemUsage;

namespace Presentation.Web.Controllers.API.V2.Internal.OrganizationUnits
{
    [InternalApi]
    [RoutePrefix("api/v1/organizations/{organizationUuid}/organization-units/{unitUuid}/registrations")]
    public class OrganizationUnitRegistrationInternalV2Controller : InternalApiV2Controller
    {
        private readonly IOrganizationUnitService _organizationUnitService;

        public OrganizationUnitRegistrationInternalV2Controller(IOrganizationUnitService organizationUnitService)
        {
            _organizationUnitService = organizationUnitService;
        }

        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetRegistrations(Guid organizationUuid, Guid unitUuid)
        {
            return _organizationUnitService.GetRegistrations(organizationUuid, unitUuid)
                .Select(ToRegistrationDto)
                .Match(Ok, FromOperationError);
        }

        [HttpDelete]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult RemoveRegistrations(Guid organizationUuid, Guid unitUuid, [FromBody] ChangeOrganizationUnitRegistrationV2RequestDTO requestDto)
        {
            var changeParameters = ToChangeParameters(requestDto);
            return _organizationUnitService.DeleteRegistrations(organizationUuid, unitUuid, changeParameters)
                .Match(FromOperationError, Ok);
        }

        [HttpPut]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult TransferRegistrations(Guid organizationUuid, Guid unitUuid, [FromBody] TransferOrganizationUnitRegistrationV2RequestDTO requestDto)
        {
            var changeParameters = ToChangeParameters(requestDto);
            return _organizationUnitService.TransferRegistrations(organizationUuid, unitUuid, requestDto.TargetUnitUuid, changeParameters)
                .Match(FromOperationError, Ok);
        }

        private static OrganizationRegistrationUnitResponseDTO ToRegistrationDto(OrganizationUnitRegistrationDetails details)
        {
            return new OrganizationRegistrationUnitResponseDTO
            {
                OrganizationUnitRights = details.OrganizationUnitRights.Select(MapUnitRightToNamedEntityDtoWithUserFullNameDto).ToList(),
                ItContractRegistrations = details.ItContractRegistrations.Select(MapToNamedEntityDTO).ToList(),
                Payments = details.PaymentRegistrationDetails.Select(ToPaymentRegistrationDto).ToList(),
                RelevantSystems = details.RelevantSystems.Select(MapToNamedEntityWithEnabledStatusDTO).ToList(),
                ResponsibleSystems = details.ResponsibleSystems.Select(MapToNamedEntityWithEnabledStatusDTO).ToList()
            };
        }

        private static PaymentRegistrationResponseDTO ToPaymentRegistrationDto(PaymentRegistrationDetails details)
        {
            return new PaymentRegistrationResponseDTO
            {
                ItContract = details.ItContract.MapIdentityNamePairDTO(),
                InternalPayments = details.InternalPayments.Select(MapPaymentToNamedEntityDto).ToList(),
                ExternalPayments = details.ExternalPayments.Select(MapPaymentToNamedEntityDto).ToList()
            };
        }

        private static NamedEntityWithUserFullNameV2DTO MapUnitRightToNamedEntityDtoWithUserFullNameDto(OrganizationUnitRight right)
        {
            return new NamedEntityWithUserFullNameV2DTO
            {
                Id = right.Id,
                Name = right.Role.Name,
                UserFullName = right.User.GetFullName()
            };
        }

        private static NamedEntityV2DTO MapPaymentToNamedEntityDto(EconomyStream payment)
        {
            return new NamedEntityV2DTO
            {
                Id = payment.Id,
                Name = $"{payment.Acquisition}, {payment.Operation}, {payment.Other}"
            };
        }

        private static OrganizationUnitRegistrationChangeParameters ToChangeParameters(
            ChangeOrganizationUnitRegistrationV2RequestDTO requestDto)
        {
            return new OrganizationUnitRegistrationChangeParameters
            (
                requestDto.OrganizationUnitRights,
                requestDto.ItContractRegistrations,
                requestDto.PaymentRegistrationDetails.Select(x =>
                    new PaymentChangeParameters(x.ItContractId, x.InternalPayments, x.ExternalPayments)),
                requestDto.ResponsibleSystems,
                requestDto.RelevantSystems
            );
        }

        private static NamedEntityV2DTO MapToNamedEntityDTO(ItContract source)
        {
            return new NamedEntityV2DTO(source.Id, source.Name);
        }

        public static NamedEntityWithEnabledStatusV2DTO MapToNamedEntityWithEnabledStatusDTO(ItSystemUsage source)
        {
            return new NamedEntityWithEnabledStatusV2DTO(source.Id, source.ItSystem.Name, source.ItSystem.Disabled);
        }
    }
}