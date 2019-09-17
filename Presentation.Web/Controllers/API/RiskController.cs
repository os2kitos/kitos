using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Core.DomainServices.Repositories.Project;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class RiskController : GenericContextAwareApiController<Risk, RiskDTO>
    {
        private readonly IItProjectRepository _projectRepository;

        public RiskController(
            IGenericRepository<Risk> repository,
            IAuthorizationContext authorizationContext,
            IItProjectRepository projectRepository)
            : base(repository, authorizationContext)
        {
            _projectRepository = projectRepository;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<RiskDTO>>))]
        public HttpResponseMessage GetByProject(bool? getByProject, int projectId)
        {
            try
            {
                var risks = Repository.Get(r => r.ItProjectId == projectId);

                return Ok(Map(risks));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<Risk>(x => _projectRepository.GetById(x.ItProjectId), base.GetCrudAuthorization());
        }
    }
}
