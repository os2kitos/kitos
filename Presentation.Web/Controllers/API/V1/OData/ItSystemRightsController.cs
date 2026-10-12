using System.Linq;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using System.Net;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel.Events;
using Core.DomainModel.ItSystemUsage;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [InternalApi]
    public class ItSystemRightsController : BaseEntityController<ItSystemRight>
    {
        private readonly IItSystemUsageService _systemUsageService;

        public ItSystemRightsController(
            IGenericRepository<ItSystemRight> repository,
            IItSystemUsageService systemUsageService)
            : base(repository)
        {
            _systemUsageService = systemUsageService;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<ItSystemRight, ItSystemUsage>(
                sr => _systemUsageService.GetById(sr.ObjectId), base.GetCrudAuthorization());
        }

        protected override IQueryable<ItSystemRight> GetAllQuery()
        {
            var all = base.GetAllQuery();
            if (UserContext.IsGlobalAdmin())
                return all;
            var orgIds = UserContext.OrganizationIds.ToList();
            return all.Where(x => orgIds.Contains(x.Object.OrganizationId));
        }

        [EnableQuery]
        [Route("odata/Users({userId})/ItSystemRights")]
        [RequireTopOnOdataThroughKitosToken]
        public IActionResult GetByUser(int userId)
        {
            var result = GetAllQuery().Where(x => x.UserId == userId);

            return Ok(result.ToList());
        }

        protected override void RaiseCreatedDomainEvent(ItSystemRight entity)
        {
            base.RaiseCreatedDomainEvent(entity);
            RaiseRootUpdated(entity);
        }

        protected override void RaiseDeletedDomainEvent(ItSystemRight entity)
        {
            base.RaiseDeletedDomainEvent(entity);
            RaiseRootUpdated(entity);
        }

        protected override void RaiseUpdatedDomainEvent(ItSystemRight entity)
        {
            base.RaiseUpdatedDomainEvent(entity);
            RaiseRootUpdated(entity);
        }

        private void RaiseRootUpdated(ItSystemRight entity)
        {
            var root = entity.Object ?? _systemUsageService.GetById(entity.ObjectId);
            if (root != null)
                DomainEvents.Raise(new EntityUpdatedEvent<ItSystemUsage>(root));
        }
    }
}




