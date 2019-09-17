using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
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
    public class CommunicationController : GenericContextAwareApiController<Communication, CommunicationDTO>
    {
        private readonly IItProjectRepository _projectRepository;

        public CommunicationController(
            IGenericRepository<Communication> repository, 
            IAuthorizationContext authorization, 
            IItProjectRepository projectRepository) 
            : base(repository, authorization)
        {
            _projectRepository = projectRepository;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<CommunicationDTO>>))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetSingle(int id, [FromUri] bool project)
        {
            var item = Repository.Get(x => x.ItProjectId == id);
            
            if (item == null)
                return NotFound();

            return Ok(Map(item));
        }

        protected override bool AllowCreate<T>(IEntity entity)
        {
            if (entity is Communication relation)
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
            if (entity is Communication relation)
            {
                return condition.Invoke(relation.ItProject);
            }

            return false;
        }
    }
}
