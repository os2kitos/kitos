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
        [Route("{unitId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetUnitRegistrations(int unitId)
        {
            return _organizationUnitService.GetOrganizationRegistrations(unitId)
                .Match
                (
                    value => Ok(ToRegistrationDto(value)),
                    err =>
                    {
                        return err.FailureType switch
                        {
                            OperationFailure.Forbidden => Unauthorized(err.Message.GetValueOrDefault()),
                            OperationFailure.BadInput => BadRequest(err.Message.GetValueOrDefault()),
                            _ => BadRequest()
                        };
                    }
                );
        }

        [HttpDelete]
        [Route("{unitId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage RemoveSelectedUnitRegistrations(int unitId, [FromBody] ChangeOrganizationRegistrationRequest request)
        {
            var changeParameters = ToChangeParameters(request);
            return _organizationUnitService.DeleteSelectedOrganizationRegistrations(unitId, changeParameters)
                .Match(error => error.FailureType switch
                {
                    OperationFailure.NotFound => NotFound(),
                    OperationFailure.BadInput => BadRequest(error.Message.GetValueOrDefault()),
                    OperationFailure.BadState => BadRequest(error.Message.GetValueOrDefault()),
                    OperationFailure.Forbidden => Forbidden(),
                }, Ok);
        }

        [HttpDelete]
        [Route("unit/{unitId}/{organizationId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage DeleteUnitWithRegistrations(int unitId, int organizationId)
        {
            return _organizationUnitService.DeleteUnitWithOrganizationRegistrations(unitId, organizationId)
                .Match(error =>error.FailureType switch
                        {
                            OperationFailure.NotFound => NotFound(),
                            OperationFailure.BadInput => BadRequest(error.Message.GetValueOrDefault()),
                            OperationFailure.BadState => BadRequest(error.Message.GetValueOrDefault()),
                            OperationFailure.Forbidden => Forbidden(),
                            _ => BadRequest()
                        }, Ok);
        }

        [HttpPut]
        [Route("{unitId}/{targetUnitId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage TransferSelectedUnitRegistrations(int unitId, int targetUnitId, [FromBody] ChangeOrganizationRegistrationRequest request)
        {
            var changeParameters = ToChangeParameters(request);
            return _organizationUnitService.TransferSelectedOrganizationRegistrations(unitId, targetUnitId, changeParameters)
                .Match(error => error.FailureType switch
                {
                    OperationFailure.NotFound => NotFound(),
                    OperationFailure.BadInput => BadRequest(error.Message.GetValueOrDefault()),
                    OperationFailure.BadState => BadRequest(error.Message.GetValueOrDefault()),
                    OperationFailure.Forbidden => Forbidden(),
                    _ => BadRequest()
                }, Ok);
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
            ChangeOrganizationRegistrationRequest request)
        {
            return new OrganizationRegistrationChangeParameters
            {
                ItContractRegistrations = request.ItContractRegistrations,
                OrganizationUnitRights = request.OrganizationUnitRights,
                PaymentRegistrationDetails = request.PaymentRegistrationDetails.Select(x =>
                    new PaymentChangeParameters(x.ItContractId, x.InternalPayments, x.ExternalPayments)),
                RelevantSystems = request.RelevantSystems,
                ResponsibleSystems = request.ResponsibleSystems
            };
        }
    }
}