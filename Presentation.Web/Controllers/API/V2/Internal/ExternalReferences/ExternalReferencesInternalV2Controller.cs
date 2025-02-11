using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.GDPR;
using Core.ApplicationServices.System;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Response.ExternalReferences;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V2.Internal.ExternalReferences
{
    [RoutePrefix("api/v2/internal/external-references")]
    public class ExternalReferencesInternalV2Controller : InternalApiV2Controller
    {
        private readonly IItSystemService _systemService;
        private readonly IItSystemUsageService _usageService;
        private readonly IItContractService _contractService;
        private readonly IDataProcessingRegistrationApplicationService _dprService;

        public ExternalReferencesInternalV2Controller(IItSystemService systemService, IItSystemUsageService usageService, IItContractService contractService, IDataProcessingRegistrationApplicationService dprService)
        {
            _systemService = systemService;
            _usageService = usageService;
            _contractService = contractService;
            _dprService = dprService;
        }

        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ExternalReferenceWithLastChangedResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("it-systems/{systemUuid}")]
        public IHttpActionResult GetItSystemReferences([NonEmptyGuid][FromUri] Guid systemUuid)
        {
            return _systemService.GetSystem(systemUuid)
                .Select(ToResponseDTO)
                .Match(Ok, FromOperationError);
        }

        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ExternalReferenceWithLastChangedResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("it-system-usages/{systemUsageUuid}")]
        public IHttpActionResult GetItSystemUsageReferences([NonEmptyGuid][FromUri] Guid systemUsageUuid)
        {
            return _usageService.GetReadableItSystemUsageByUuid(systemUsageUuid)
                .Select(ToResponseDTO)
                .Match(Ok, FromOperationError);
        }

        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ExternalReferenceWithLastChangedResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("it-contracts/{contractUuid}")]
        public IHttpActionResult GetItContractReferences([NonEmptyGuid][FromUri] Guid contractUuid)
        {
            return _contractService.GetContract(contractUuid)
                .Select(ToResponseDTO)
                .Match(Ok, FromOperationError);
        }

        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ExternalReferenceWithLastChangedResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("data-processing/{dprUuid}")]
        public IHttpActionResult GetDataProcessingReferences([NonEmptyGuid][FromUri] Guid dprUuid)
        {
            return _dprService.GetByUuid(dprUuid)
                .Select(ToResponseDTO)
                .Match(Ok, FromOperationError);
        }

        private IEnumerable<ExternalReferenceWithLastChangedResponseDTO> ToResponseDTO(IHasReferences entity)
        {
            return entity.ExternalReferences.Select(MapExternalReferenceWithLastChanged).ToList();
        }

        private static ExternalReferenceWithLastChangedResponseDTO MapExternalReferenceWithLastChanged(ExternalReference externalReference)
        {
            return new ExternalReferenceWithLastChangedResponseDTO
            {
                Uuid = externalReference.Uuid,
                DocumentId = externalReference.ExternalReferenceId,
                Title = externalReference.Title,
                Url = externalReference.URL,
                MasterReference = externalReference.IsMasterReference(),
                LastChangedByUsername = externalReference.LastChangedByUser.GetFullName(),
                LastChangedDate = externalReference.LastChanged
            };
        }
    }
}