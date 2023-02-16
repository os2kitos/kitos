using System.Linq;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.OData;
using Swashbuckle.Swagger.Annotations;
using System.Net;
using Core.DomainModel.Events;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Core.DomainModel.GDPR;
using Core.DomainServices.Repositories.GDPR;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [InternalApi]
    public class DataProcessingRegistrationRightsController : BaseEntityController<DataProcessingRegistrationRight>
    {
        private readonly IDataProcessingRegistrationRepository _dataProcessingRegistrationRepository;

        public DataProcessingRegistrationRightsController(IGenericRepository<DataProcessingRegistrationRight> repository, IDataProcessingRegistrationRepository dataProcessingRegistrationRepository)
            : base(repository)
        {
            _dataProcessingRegistrationRepository = dataProcessingRegistrationRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            // ChildEntityCrudAuthorization uses FromNullable when accessing the object. So even if .Value returns null it will be resolved.
            return new ChildEntityCrudAuthorization<DataProcessingRegistrationRight, DataProcessingRegistration>(r => _dataProcessingRegistrationRepository.GetById(r.ObjectId).Value, base.GetCrudAuthorization());
        }

        // GET /Users(1)/DataProcessingRegistrationRights
        [EnableQuery]
        [ODataRoute("Users({userId})/DataProcessingRegistrationRights")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IQueryable<DataProcessingRegistrationRight>>))]
        [RequireTopOnOdataThroughKitosToken]
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

        protected override IQueryable<DataProcessingRegistrationRight> GetAllQuery()
        {
            var all = base.GetAllQuery();
            if (UserContext.IsGlobalAdmin())
                return all;
            var orgIds = UserContext.OrganizationIds.ToList();
            return all.Where(x => orgIds.Contains(x.Object.OrganizationId));
        }

        protected override void RaiseCreatedDomainEvent(DataProcessingRegistrationRight entity)
        {
            base.RaiseCreatedDomainEvent(entity);
            RaiseRootUpdated(entity);
        }

        protected override void RaiseDeletedDomainEvent(DataProcessingRegistrationRight entity)
        {
            base.RaiseDeletedDomainEvent(entity);
            RaiseRootUpdated(entity);
        }

        protected override void RaiseUpdatedDomainEvent(DataProcessingRegistrationRight entity)
        {
            base.RaiseUpdatedDomainEvent(entity);
            RaiseRootUpdated(entity);
        }

        private void RaiseRootUpdated(DataProcessingRegistrationRight entity)
        {
            var root = entity.Object ?? _dataProcessingRegistrationRepository.GetById(entity.ObjectId).GetValueOrDefault();
            if (root != null)
                DomainEvents.Raise(new EntityUpdatedEvent<DataProcessingRegistration>(root));
        }
    }
}
