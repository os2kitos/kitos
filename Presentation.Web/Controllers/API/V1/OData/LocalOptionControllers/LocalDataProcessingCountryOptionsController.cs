﻿using System.Web.Http;
using Core.DomainModel.GDPR;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData.LocalOptionControllers
{
    [InternalApi]
    [ODataRoutePrefix("LocalDataProcessingCountryOptions")]
    public class LocalDataProcessingCountryOptionsController : LocalOptionBaseController<LocalDataProcessingCountryOption, DataProcessingRegistration, DataProcessingCountryOption>
    {
        public LocalDataProcessingCountryOptionsController(IGenericRepository<LocalDataProcessingCountryOption> repository, IGenericRepository<DataProcessingCountryOption> optionsRepository)
            : base(repository, optionsRepository)
        {

        }

        [EnableQuery]
        [ODataRoute]
        [RequireTopOnOdataThroughKitosToken]
        public override IHttpActionResult GetByOrganizationId(int organizationId) => base.GetByOrganizationId(organizationId);

        [EnableQuery]
        [ODataRoute]
        public override IHttpActionResult Get(int organizationId, int key) => base.Get(organizationId, key);

        [ODataRoute]
        public override IHttpActionResult Post(int organizationId, LocalDataProcessingCountryOption entity) => base.Post(organizationId, entity);

        [ODataRoute]
        public override IHttpActionResult Patch(int organizationId, int key, Delta<LocalDataProcessingCountryOption> delta) => base.Patch(organizationId, key, delta);

        [ODataRoute]
        public override IHttpActionResult Delete(int organizationId, int key) => base.Delete(organizationId, key);
    }
}
