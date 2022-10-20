using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.Organizations;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1.Organizations;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V1
{
    [PublicApi]
    [RoutePrefix("api/v1/organization-registrations")]

    public class OrganizationRegistrationController: BaseApiController
    {
        private readonly IOrganizationRegistrationService _organizationRegistrationService;

        public OrganizationRegistrationController(IOrganizationRegistrationService organizationRegistrationService)
        {
            _organizationRegistrationService = organizationRegistrationService;
        }

        [HttpGet]
        [Route("{unitId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetUnitRegistrations(int unitId)
        {
            return _organizationRegistrationService.GetOrganizationRegistrations(unitId)
                .Match(Ok,
                    err =>
                    {
                        return err.FailureType switch
                        {
                            OperationFailure.Forbidden => Unauthorized(err.Message.GetValueOrDefault()),
                            OperationFailure.BadInput => BadRequest(err.Message.GetValueOrDefault()),
                            _ => BadRequest()
                        };
                    });
        }

        [HttpDelete]
        [Route("{unitId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage RemoveSelectedUnitRegistrations(int unitId, [FromBody] ChangeOrganizationRegistrationsRequest request)
        {
            var changeParameters = ToChangeParameters(request);

            return _organizationRegistrationService.DeleteSelectedOrganizationRegistrations(unitId, changeParameters)
                .Match(error => error.FailureType switch
                {
                    OperationFailure.Forbidden => Unauthorized(),
                    OperationFailure.BadInput => BadRequest(error.Message.GetValueOrDefault()),
                }, Ok);
        }

        [HttpDelete]
        [Route("unit/{unitId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage DeleteRegistrations(int unitId)
        {
            return _organizationRegistrationService.DeleteUnitWithOrganizationRegistrations(unitId)
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
        public HttpResponseMessage TransferSelectedUnitRegistrations(int unitId, int targetUnitId, [FromBody] ChangeOrganizationRegistrationsRequest request)
        {
            var changeParameters = ToChangeParameters(request);
            return _organizationRegistrationService.TransferSelectedOrganizationRegistrations(unitId, targetUnitId, changeParameters)
                .Match(error => error.FailureType switch
                {
                    OperationFailure.NotFound => NotFound(),
                    OperationFailure.BadInput => BadRequest(error.Message.GetValueOrDefault()),
                    OperationFailure.BadState => BadRequest(error.Message.GetValueOrDefault()),
                    OperationFailure.Forbidden => Forbidden(),
                    _ => BadRequest()
                }, Ok);
        }

        private OrganizationRegistrationsChangeParameters ToChangeParameters(
            ChangeOrganizationRegistrationsRequest request)
        {
            return new OrganizationRegistrationsChangeParameters
            {
                RoleIds = request.Roles,
                InternalPaymentIds = request.InternalPayments,
                ExternalPaymentIds = request.ExternalPayments,
                ContractWithRegistrationIds = request.ContractRegistrations,
                ResponsibleSystemIds = request.ResponsibleSystems,
                RelevantSystems = request.RelevantSystems
            };
        }
    }
}