using Core.ApplicationServices.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.Abstractions.Types;
using Core.ApplicationServices.Contract.Write;
using Core.DomainModel.Extensions;
using Core.DomainModel.ItContract;
using Core.DomainServices.Generic;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Swashbuckle.Swagger.Annotations;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Controllers.API.V2.External.ItContracts.Mapping;
using Presentation.Web.Controllers.API.V2.Internal.Mapping;
using Presentation.Web.Models.API.V2.Response.Generic.Hierarchy;
using Presentation.Web.Models.API.V2.Internal.Response.Roles;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Response.Contract;
using Presentation.Web.Models.API.V2.Internal.Response.ItContract;

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
        private readonly IItContractWriteService _writeService;
        private readonly IItContractResponseMapper _responseMapper;

        public ItContractInternalV2Controller(IItContractService itContractService,
            IEntityIdentityResolver identityResolver,
            IItContractWriteService writeService,
            IItContractResponseMapper responseMapper)
        {
            _itContractService = itContractService;
            _identityResolver = identityResolver;
            _writeService = writeService;
            _responseMapper = responseMapper;
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
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ItContractHierarchyNodeResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetHierarchy([NonEmptyGuid] Guid contractUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _itContractService.GetContract(contractUuid)
                .Select(contract => contract.FlattenCompleteHierarchy())
                .Select(RegistrationHierarchyNodeMapper.MapContractHierarchyToDtos)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Get roles assigned to the contract
        /// </summary>
        /// <param name="contractUuid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{contractUuid}/roles")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ExtendedRoleAssignmentResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetAddRoleAssignments([NonEmptyGuid] Guid contractUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return _itContractService
                .GetContract(contractUuid)
                .Select(x => x.Rights.ToList())
                .Select(rights => rights.Select(right => right.MapExtendedRoleAssignmentResponse()))
                .Match(Ok, FromOperationError);
        }
        /// Add role assignment to the it-contract
        /// Constraint: Duplicates are not allowed (existing assignment of the same user/role)
        /// </summary>
        /// <param name="contractUuid"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{contractUuid}/roles/add")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItContractResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Conflict, Description = "If duplicate is detected")]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PatchAddRoleAssignment([NonEmptyGuid] Guid contractUuid, [FromBody] RoleAssignmentRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _writeService
                .AddRole(contractUuid, request.ToUserRolePair())
                .Select(_responseMapper.MapContractDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Remove an existing role assignment to the it-contract
        /// </summary>
        /// <param name="contractUuid"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{contractUuid}/roles/remove")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItContractResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PatchRemoveRoleAssignment([NonEmptyGuid] Guid contractUuid, [FromBody] RoleAssignmentRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _writeService
                .RemoveRole(contractUuid, request.ToUserRolePair())
                .Select(_responseMapper.MapContractDTO)
                .Match(Ok, FromOperationError);
        }

        [HttpGet]
        [Route("applied-procurement-plans/{organizationUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<AppliedProcurementPlanResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetAppliedProcurementPlans([NonEmptyGuid][FromUri] Guid organizationUuid)
        {
            return _itContractService.GetAppliedProcurementPlansByUuid(organizationUuid)
                .Select(plans => plans.Select(MapAppliedProcurementPlansToDTO))
                .Match(Ok, FromOperationError);
        }

        private static AppliedProcurementPlanResponseDTO MapAppliedProcurementPlansToDTO((int, int) procurementTuple)
        {
            return new AppliedProcurementPlanResponseDTO(procurementTuple.Item1, procurementTuple.Item2);
        }
    }
}