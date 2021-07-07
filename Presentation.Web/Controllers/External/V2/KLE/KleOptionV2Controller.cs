using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.KLE;
using Core.DomainModel.Organization;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.KLE;
using Infrastructure.Services.Types;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.External.V2.Request;
using Presentation.Web.Models.External.V2.Response.KLE;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2.KLE
{
    /// <summary>
    /// Returns the available KLE options in KITOS
    /// </summary>
    [RoutePrefix("api/v2/kle-options")]
    public class KleOptionV2Controller : ExternalBaseController
    {
        private readonly IKLEApplicationService _kleApplicationService;

        public KleOptionV2Controller(IKLEApplicationService kleApplicationService)
        {
            _kleApplicationService = kleApplicationService;
        }

        /// <summary>
        /// Returns the KLE reference used by KITOS
        /// </summary>
        /// <param name="parentKleUuid">Query by parent KLE uuid</param>
        /// <param name="parentKleNumber">Query by parent KLE number (exact match)</param>
        /// <param name="kleNumberPrefix">Query by KLE number prefix</param>
        /// <param name="kleDescriptionContent">Query by KLE description content</param>
        /// <param name="kleCategory">Query by KLE category</param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(VersionedKLEResponseDTO<IEnumerable<KLEDetailsDTO>>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult Get(
            [NonEmptyGuid] Guid? parentKleUuid = null,
            string parentKleNumber = null,
            string kleNumberPrefix = null,
            string kleDescriptionContent = null,
            [FromUri] StandardPaginationQuery pagination = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var criteria = new List<IDomainQuery<TaskRef>>();

            if (parentKleUuid.HasValue)
                criteria.Add(new QueryByParentUuid(parentKleUuid.Value));

            if (!string.IsNullOrWhiteSpace(parentKleNumber))
                criteria.Add(new QueryByParentKey(parentKleNumber));

            if (!string.IsNullOrWhiteSpace(kleNumberPrefix))
                criteria.Add(new QueryByKeyPrefix(kleNumberPrefix));

            if (!string.IsNullOrWhiteSpace(kleDescriptionContent))
                criteria.Add(new QueryByDescriptionContent(kleDescriptionContent));

            return _kleApplicationService
                .SearchKle(criteria.ToArray())
                .Select(result => (result.updateReference, result.contents.OrderBy(x => x.Id).Page(pagination).ToList()))
                .Select(result => new VersionedKLEResponseDTO<IEnumerable<KLEDetailsDTO>>
                {
                    ReferenceVersion = result.updateReference.GetValueOrFallback(DateTime.MinValue),
                    Payload = result.Item2.Select(ToDTO).ToList()
                })
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Returns the details of a single KLE
        /// </summary>
        /// <param name="kleUuid">UUID of the KLE number</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{kleUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(VersionedKLEResponseDTO<KLEDetailsDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "kleUuid is invalid")]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid kleUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _kleApplicationService.GetKle(kleUuid).Select(result => new VersionedKLEResponseDTO<KLEDetailsDTO>()
            {
                ReferenceVersion = result.updateReference.GetValueOrFallback(DateTime.MinValue),
                Payload = result.kle.Transform(ToDTO)
            }).Match(Ok, FromOperationError);
        }

        private static KLEDetailsDTO ToDTO(TaskRef taskRef)
        {
            return new()
            {
                Description = taskRef.Description,
                KleNumber = taskRef.TaskKey,
                ParentKle = taskRef.Parent?.MapIdentityNamePairDTO(),
                Uuid = taskRef.Uuid
            };
        }
    }
}