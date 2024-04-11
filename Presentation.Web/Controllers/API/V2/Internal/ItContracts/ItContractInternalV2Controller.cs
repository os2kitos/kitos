using Core.ApplicationServices.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.Abstractions.Types;
using Core.DomainModel.Extensions;
using Core.DomainModel.ItContract;
using Core.DomainServices.Generic;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Swashbuckle.Swagger.Annotations;
using Presentation.Web.Models.API.V2.Response.Generic.Hierarchy;
using Presentation.Web.Controllers.API.V2.External.Generic;

namespace Presentation.Web.Controllers.API.V2.Internal.ItContracts
{
    /// <summary>
    /// Internal API for the it contracts
    /// </summary>
    [RoutePrefix("api/v2/internal/it-contracts")]
    public class ItContractInternalV2Controller : InternalApiV2Controller
    {
        private readonly IItContractService _itContractService;
        private readonly IEntityIdentityResolver _identityResolver;

        public ItContractInternalV2Controller(IItContractService itContractService, IEntityIdentityResolver identityResolver)
        {
            _itContractService = itContractService;
            _identityResolver = identityResolver;
        }

        [HttpGet]
        [Route("{contractUuid}/data-processing-registrations")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<IdentityNamePairResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetDataProcessingRegistrations(
            [NonEmptyGuid] Guid contractUuid,
            string nameQuery = null, [FromUri] int pageSize = 25)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _identityResolver.ResolveDbId<ItContract>(contractUuid)
                .Match(id => _itContractService.GetDataProcessingRegistrationsWhichCanBeAssigned(id,
                        nameQuery,
                        pageSize),
                    () => new OperationError($"Id couldn't be resolved for contract with uuid: {contractUuid}",
                        OperationFailure.NotFound))
                .Select(dprs => dprs.Select(dpr => dpr.MapIdentityNamePairDTO()))
                .Match(Ok, FromOperationError);
        }

        [HttpGet]
        [Route("{contractUuid}/hierarchy")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<RegistrationHierarchyNodeResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetHierarchy([NonEmptyGuid] Guid contractUuid)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            return _itContractService.GetContract(contractUuid)
                .Select(contract => contract.FlattenCompleteHierarchy())
                .Select(RegistrationHierarchyNodeMapper.MapHierarchyToDtos)
                .Match(Ok, FromOperationError);
        }
    }
}