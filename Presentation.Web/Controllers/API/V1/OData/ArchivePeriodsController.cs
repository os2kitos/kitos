using System.Linq;
using System.Web.Http;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel.Events;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [InternalApi]
    public class ArchivePeriodsController : BaseEntityController<ArchivePeriod>
    {
        private readonly IItSystemUsageService _itSystemUsageService;

        public ArchivePeriodsController(IGenericRepository<ArchivePeriod> repository, IItSystemUsageService itSystemUsageService)
            : base(repository)
        {
            _itSystemUsageService = itSystemUsageService;
        }

        protected override IQueryable<ArchivePeriod> GetAllQuery()
        {
            var query = Repository.AsQueryable();
            if (AuthorizationContext.GetCrossOrganizationReadAccess() == CrossOrganizationDataReadAccessLevel.All)
            {
                return query;
            }
            var organizationIds = UserContext.OrganizationIds.ToList();

            return query.Where(x => organizationIds.Contains(x.ItSystemUsage.OrganizationId));
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<ArchivePeriod, ItSystemUsage>(ap => _itSystemUsageService.GetById(ap.ItSystemUsageId), base.GetCrudAuthorization());
        }

        protected override void RaiseCreatedDomainEvent(ArchivePeriod entity)
        {
            var itSystemUsage = _itSystemUsageService.GetById(entity.ItSystemUsageId);
            if (itSystemUsage != null)
            {
                DomainEvents.Raise(new EntityUpdatedEvent<ItSystemUsage>(itSystemUsage));
            }
        }

        protected override void RaiseUpdatedDomainEvent(ArchivePeriod entity)
        {
            DomainEvents.Raise(new EntityUpdatedEvent<ItSystemUsage>(entity.ItSystemUsage));
        }

        protected override void RaiseDeletedDomainEvent(ArchivePeriod entity)
        {
            var itSystemUsage = _itSystemUsageService.GetById(entity.ItSystemUsageId);
            if (itSystemUsage != null)
            {
                DomainEvents.Raise(new EntityUpdatedEvent<ItSystemUsage>(entity.ItSystemUsage));
            }
        }

        [RequireTopOnOdataThroughKitosToken]
        [EnableQuery]
        [ODataRoute("Organizations({organizationId})/ItSystemUsages({systemUsageId})/ArchivePeriods")]
        public IHttpActionResult GetArchivePeriodsForItSystemUsage(int organizationId, int systemUsageId)
        {
            if (GetOrganizationReadAccessLevel(organizationId) != OrganizationDataReadAccessLevel.All)
                return Forbidden();

            var itSystemUsage = _itSystemUsageService.GetById(systemUsageId);
            if (itSystemUsage == null)
            {
                return NotFound();
            }

            if (!CrudAuthorization.AllowRead(itSystemUsage))
            {
                return Forbidden();
            }

            return Ok(itSystemUsage.ArchivePeriods.AsQueryable());
        }
    }
}