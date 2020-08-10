﻿using System.Web.Http;
using Core.DomainModel.ItSystem;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [PublicApi]
    [ODataRoutePrefix("LocalItSystemRoles")]
    public class LocalItSystemRolesController : LocalOptionBaseController<LocalItSystemRole, ItSystemRight, ItSystemRole>
    {
        public LocalItSystemRolesController(IGenericRepository<LocalItSystemRole> repository, IGenericRepository<ItSystemRole> optionsRepository)
            : base(repository, optionsRepository)
        {

        }

        [EnableQuery]
        [ODataRoute]
        public override IHttpActionResult GetByOrganizationId(int organizationId) => base.GetByOrganizationId(organizationId);

        [EnableQuery]
        [ODataRoute]
        public override IHttpActionResult Get(int organizationId, int key) => base.Get(organizationId, key);

        [ODataRoute]
        public override IHttpActionResult Post(int organizationId, LocalItSystemRole entity) => base.Post(organizationId, entity);

        [ODataRoute]
        public override IHttpActionResult Patch(int organizationId, int key, Delta<LocalItSystemRole> delta) => base.Patch(organizationId, key, delta);

        [ODataRoute]
        public override IHttpActionResult Delete(int organizationId, int key) => base.Delete(organizationId, key);
    }
}
