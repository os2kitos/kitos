using System;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.Archive;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [InternalApi]
    public class ItSystemUsageArchivesController : BaseEntityController<ItSystemUsageArchive>
    {
        private readonly IEntityIdentityResolver _identityResolver;

        public ItSystemUsageArchivesController(IGenericRepository<ItSystemUsageArchive> repository, IEntityIdentityResolver identityResolver)
            : base(repository)
        {
            _identityResolver = identityResolver;
        }

        [EnableQuery]
        [Route("odata/ItSystemUsageArchives")]
        [RequireTopOnOdataThroughKitosToken]
        public IActionResult Get([FromQuery] Guid organizationUuid)
        {
            var orgDbId = _identityResolver.ResolveDbId<Organization>(organizationUuid);
            if (orgDbId.IsNone)
            {
                return FromOperationError(new OperationError("Invalid org id", OperationFailure.NotFound));
            }

            var accessLevel = GetOrganizationReadAccessLevel(orgDbId.Value);
            if (accessLevel < OrganizationDataReadAccessLevel.Public)
            {
                return Forbidden();
            }

            return Ok(GetAllQuery().Where(x => x.OrganizationId == orgDbId.Value).ToList());
        }

        protected override IQueryable<ItSystemUsageArchive> GetAllQuery()
        {
            var all = base.GetAllQuery();
            if (!UserContext.IsGlobalAdmin())
            {
                var orgIds = UserContext.OrganizationIds.ToList();
                all = all.Where(x => orgIds.Contains(x.OrganizationId));
            }

            return all;
        }

        [NonAction]
        public override IActionResult Get() => throw new NotSupportedException();

        [NonAction]
        public override IActionResult Get(int key) => throw new NotSupportedException();

        [NonAction]
        public override IActionResult Post(int organizationId, ItSystemUsageArchive entity) => throw new NotSupportedException();

        [NonAction]
        public override IActionResult Patch(int key, Delta<ItSystemUsageArchive> delta) => throw new NotSupportedException();

        [NonAction]
        public override IActionResult Delete(int key) => throw new NotSupportedException();
    }
}
