using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.GDPR;
using Core.ApplicationServices.System;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Response.ExternalReferences;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Web.Controllers.API.V2.Internal.ExternalReferences
{
    [Route("api/v2/internal/external-references")]
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
        [Route("it-systems/{systemUuid}")]
        [ApiResponse(typeof(IEnumerable<ExternalReferenceWithLastChangedResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetItSystemReferences([NonEmptyGuid][FromRoute] Guid systemUuid)
        {
            return _systemService.GetSystem(systemUuid)
                .Select(ToResponseDTO)
                .Match(Ok, FromOperationError);
        }

        [HttpGet]
        [Route("it-system-usages/{systemUsageUuid}")]
        [ApiResponse(typeof(IEnumerable<ExternalReferenceWithLastChangedResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetItSystemUsageReferences([NonEmptyGuid][FromRoute] Guid systemUsageUuid)
        {
            return _usageService.GetItSystemUsageByUuidAndAuthorizeRead(systemUsageUuid)
                .Select(ToResponseDTO)
                .Match(Ok, FromOperationError);
        }

        [HttpGet]
        [Route("it-contracts/{contractUuid}")]
        [ApiResponse(typeof(IEnumerable<ExternalReferenceWithLastChangedResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetItContractReferences([NonEmptyGuid][FromRoute] Guid contractUuid)
        {
            return _contractService.GetContract(contractUuid)
                .Select(ToResponseDTO)
                .Match(Ok, FromOperationError);
        }

        [HttpGet]
        [Route("data-processing/{dprUuid}")]
        [ApiResponse(typeof(IEnumerable<ExternalReferenceWithLastChangedResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetDataProcessingReferences([NonEmptyGuid][FromRoute] Guid dprUuid)
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

