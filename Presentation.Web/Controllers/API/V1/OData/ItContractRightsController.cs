using System.Linq;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Core.DomainServices;
using Core.DomainModel.ItContract;
using Core.DomainServices.Repositories.Contract;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Swashbuckle.OData;
using Swashbuckle.Swagger.Annotations;
using System.Net;
using Core.DomainModel.Events;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [InternalApi]
    public class ItContractRightsController : BaseEntityController<ItContractRight>
    {
        private readonly IItContractRepository _itContractRepository;

        public ItContractRightsController(IGenericRepository<ItContractRight> repository, IItContractRepository itContractRepository)
            : base(repository)
        {
            _itContractRepository = itContractRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<ItContractRight, ItContract>(r => _itContractRepository.GetById(r.ObjectId), base.GetCrudAuthorization());
        }

        // GET /Users(1)/ItContractRights
        [EnableQuery]
        [ODataRoute("Users({userId})/ItContractRights")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IQueryable<ItContractRight>>))]
        [RequireTopOnOdataThroughKitosToken]
        public IHttpActionResult GetByUser(int userId)
        {
            var result = GetAllQuery().Where(x => x.UserId == userId);

            return Ok(result);
        }

        protected override IQueryable<ItContractRight> GetAllQuery()
        {
            var all = base.GetAllQuery();
            if (UserContext.IsGlobalAdmin())
                return all;
            var orgIds = UserContext.OrganizationIds.ToList();
            return all.Where(x => orgIds.Contains(x.Object.OrganizationId));
        }

        protected override void RaiseCreatedDomainEvent(ItContractRight entity)
        {
            base.RaiseCreatedDomainEvent(entity);
            RaiseRootUpdated(entity);
        }

        protected override void RaiseDeletedDomainEvent(ItContractRight entity)
        {
            base.RaiseDeletedDomainEvent(entity);
            RaiseRootUpdated(entity);
        }

        protected override void RaiseUpdatedDomainEvent(ItContractRight entity)
        {
            base.RaiseUpdatedDomainEvent(entity);
            RaiseRootUpdated(entity);
        }

        private void RaiseRootUpdated(ItContractRight entity)
        {
            var root = entity.Object ?? _itContractRepository.GetById(entity.ObjectId);
            if (root != null)
                DomainEvents.Raise(new EntityUpdatedEvent<ItContract>(root));
        }
    }
}
