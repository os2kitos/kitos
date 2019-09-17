using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.DomainServices.Repositories.SystemUsage;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class WishController : GenericContextAwareApiController<Wish, WishDTO>
    {
        private readonly IItSystemUsageRepository _usageRepository;

        public WishController(
            IGenericRepository<Wish> repository,
            IAuthorizationContext authorization,
            IItSystemUsageRepository usageRepository)
            : base(repository, authorization)
        {
            _usageRepository = usageRepository;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<WishDTO>>))]
        public HttpResponseMessage GetWishes([FromUri] int userId, [FromUri] int usageId)
        {
            var wishes = Repository.Get(x => x.ItSystemUsageId == usageId && (x.IsPublic || x.UserId == userId));
            return Ok(Map(wishes));
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<Wish>(x => _usageRepository.GetSystemUsage(x.ItSystemUsageId), base.GetCrudAuthorization());
        }
    }
}
