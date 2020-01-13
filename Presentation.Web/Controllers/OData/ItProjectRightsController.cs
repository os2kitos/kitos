using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.OData;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Net;
using Core.DomainServices.Repositories.Project;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;

namespace Presentation.Web.Controllers.OData
{
    [PublicApi]
    [MigratedToNewAuthorizationContext]
    public class ItProjectRightsController : BaseEntityController<ItProjectRight>
    {
        private readonly IItProjectRepository _itProjectRepository;

        public ItProjectRightsController(IGenericRepository<ItProjectRight> repository, IItProjectRepository itProjectRepository)
            : base(repository)
        {
            _itProjectRepository = itProjectRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<ItProjectRight>(r => _itProjectRepository.GetById(r.ObjectId), base.GetCrudAuthorization());
        }

        // GET /Users(1)/ItProjectRights
        [EnableQuery]
        [ODataRoute("Users({userId})/ItProjectRights")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IQueryable<ItProjectRight>>))]
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
