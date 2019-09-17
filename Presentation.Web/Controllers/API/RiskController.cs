using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Core.DomainServices.Repositories.Project;
using Presentation.Web.Infrastructure.Attributes;
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

        protected override bool AllowCreate<T>(IEntity entity)
        {
            if (entity is Risk relation)
            {
                var project = _projectRepository.GetById(relation.ItProjectId);
                return project != null && base.AllowModify(project);
            }
            return false;
        }

        protected override bool AllowModify(IEntity entity)
        {
            return GeAuthorizationFromRoot(entity, base.AllowModify);
        }

        protected override bool AllowDelete(IEntity entity)
        {
            //Check if modification, not deletion, of parent usage (the root aggregate) is allowed 
            return GeAuthorizationFromRoot(entity, base.AllowModify);
        }

        protected override bool AllowRead(IEntity entity)
        {
            return GeAuthorizationFromRoot(entity, base.AllowRead);
        }

        private static bool GeAuthorizationFromRoot(IEntity entity, Predicate<ItProject> condition)
        {
            if (entity is Risk relation)
            {
                return condition.Invoke(relation.ItProject);
            }

            return false;
        }
    }
}
