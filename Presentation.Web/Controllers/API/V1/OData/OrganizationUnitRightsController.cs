using Core.DomainServices;
using Core.DomainModel.Organization;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using System.Web.Http;
using System.Linq;
using Core.DomainModel.Events;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [InternalApi]
    public class OrganizationUnitRightsController : BaseEntityController<OrganizationUnitRight>
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;

        public OrganizationUnitRightsController(IGenericRepository<OrganizationUnitRight> repository, IGenericRepository<OrganizationUnit> orgUnitRepository)
            : base(repository)
        {
            _orgUnitRepository = orgUnitRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<OrganizationUnitRight, OrganizationUnit>(or => _orgUnitRepository.GetByKey(or.ObjectId), base.GetCrudAuthorization());
        }

        // GET /Users(1)/ItContractRights
        [EnableQuery]
        [ODataRoute("Users({userId})/OrganizationUnitRights")]
        public IHttpActionResult GetByUser(int userId)
        {
            var result = Repository
                .AsQueryable()
                .Where(x => x.UserId == userId)
                .AsEnumerable()
                .Where(AllowRead)
                .AsQueryable();

            return Ok(result);
        }

        protected override IQueryable<OrganizationUnitRight> GetAllQuery()
        {
            var all = base.GetAllQuery();
            if (UserContext.IsGlobalAdmin())
                return all;
            var orgIds = UserContext.OrganizationIds.ToList();
            return all.Where(x => orgIds.Contains(x.Object.OrganizationId));
        }

        protected override void RaiseCreatedDomainEvent(OrganizationUnitRight entity)
        {
            base.RaiseCreatedDomainEvent(entity);
            RaiseRootUpdated(entity);
        }

        protected override void RaiseDeletedDomainEvent(OrganizationUnitRight entity)
        {
            base.RaiseDeletedDomainEvent(entity);
            RaiseRootUpdated(entity);
        }

        protected override void RaiseUpdatedDomainEvent(OrganizationUnitRight entity)
        {
            base.RaiseUpdatedDomainEvent(entity);
            RaiseRootUpdated(entity);
        }

        private void RaiseRootUpdated(OrganizationUnitRight entity)
        {
            var root = entity.Object ?? _orgUnitRepository.GetByKey(entity.ObjectId);
            if (root != null)
                DomainEvents.Raise(new EntityUpdatedEvent<OrganizationUnit>(root));
        }
    }
}
