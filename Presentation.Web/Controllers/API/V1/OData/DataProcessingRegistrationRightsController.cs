using System.Linq;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.OData;
using Swashbuckle.Swagger.Annotations;
using System.Net;
using Core.DomainServices.Repositories.Project;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Core.DomainModel.GDPR;
using Core.DomainServices.Repositories.GDPR;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [PublicApi]
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
    }
}
