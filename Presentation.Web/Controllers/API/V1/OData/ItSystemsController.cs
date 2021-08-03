using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.System;
using Core.DomainModel.Events;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.OData;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [PublicApi]
    public class ItSystemsController : BaseEntityController<ItSystem>
    {
        private readonly IItSystemService _systemService;

        public ItSystemsController(IGenericRepository<ItSystem> repository, IItSystemService systemService)
            : base(repository)
        {
            _systemService = systemService;
        }

        /// <summary>
        /// Henter alle organisationens IT-Systemer samt offentlige IT-systemer fra andre organisationer.
        /// Resultatet filtreres i hht. brugerens læserettigheder i den opgældende organisation, samt på tværs af organisationer.
        /// </summary>
        /// <param name="orgKey"></param>
        /// <returns></returns>
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/ItSystems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IEnumerable<ItSystem>>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [RequireTopOnOdataThroughKitosToken]
        public IHttpActionResult GetItSystems(int orgKey)
        {
            var readAccessLevel = GetOrganizationReadAccessLevel(orgKey);
            if (readAccessLevel == OrganizationDataReadAccessLevel.None)
            {
                return Forbidden();
            }

            var result = Repository
                    .AsQueryable()
                    .ByOrganizationDataAndPublicDataFromOtherOrganizations(orgKey, readAccessLevel, GetCrossOrganizationReadAccessLevel());

            return Ok(result);
        }

        public override IHttpActionResult Patch(int key, Delta<ItSystem> delta)
        {
            var itSystem = Repository.GetByKey(key);

            if (itSystem == null)
                return NotFound();

            var changedPropertyNames = delta.GetChangedPropertyNames().ToHashSet();

            if (AttemptToChangeUuid(delta, itSystem, changedPropertyNames))
                return BadRequest("Cannot change Uuid");

            if (changedPropertyNames.Contains(nameof(ItSystem.Name)))
            {
                if (delta.TryGetPropertyValue(nameof(ItSystem.Name), out var name))
                {
                    if (!_systemService.CanChangeNameTo(itSystem.OrganizationId, itSystem.Id, (string)name))
                    {
                        return Conflict();
                    }
                }
            }

            var disabledBefore = itSystem.Disabled;
            var result = base.Patch(key, delta);
            if (disabledBefore != itSystem.Disabled)
            {
                DomainEvents.Raise(new EnabledStatusChanged<ItSystem>(itSystem, disabledBefore, itSystem.Disabled));
            }

            return result;
        }

        private static bool AttemptToChangeUuid(Delta<ItSystem> delta, ItSystem itSystem, HashSet<string> changedPropertyNames)
        {
            const string uuidName = nameof(Core.DomainModel.User.Uuid);

            return changedPropertyNames.Contains(uuidName) && delta.TryGetPropertyValue(uuidName, out var uuid) && ((Guid)uuid) != itSystem.Uuid;
        }


        [ODataRoute("ItSystems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IEnumerable<ItSystem>>))]
        [RequireTopOnOdataThroughKitosToken]
        public override IHttpActionResult Get()
        {
            return base.Get();
        }

        [NonAction]
        public override IHttpActionResult Delete(int key) => throw new NotSupportedException();

        [NonAction]
        public override IHttpActionResult Post(int organizationId, ItSystem entity) => throw new NotSupportedException();
    }
}
