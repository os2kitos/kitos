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
using System;
using System.Net;

namespace Presentation.Web.Controllers.OData
{
    [PublicApi]
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
