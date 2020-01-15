using Core.DomainServices;
using Core.DomainModel.Organization;
using System.Web.OData;
using System.Web.OData.Routing;
using System.Web.Http;
using System.Linq;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;

namespace Presentation.Web.Controllers.OData
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
    }
}
