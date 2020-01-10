using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [MigratedToNewAuthorizationContext]
    public class TaskRefController : GenericHierarchyApiController<TaskRef, TaskRefDTO>
    {
        public TaskRefController(IGenericRepository<TaskRef> repository, IAuthorizationContext authorizationContext)
            : base(repository, authorizationContext)
        {
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<TaskRefDTO>))]
        public HttpResponseMessage GetRootsByOrg(int orgId, bool? roots, [FromUri] PagingModel<TaskRef> paging)
        {
            paging.Where(taskRef => taskRef.OwnedByOrganizationUnit.OrganizationId == orgId || taskRef.AccessModifier == AccessModifier.Public);

            return base.GetRoots(true, paging);
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<TaskRefDTO>))]
        public HttpResponseMessage GetChildrenByOrg(int id, int orgId, bool? children, [FromUri] PagingModel<TaskRef> paging)
        {
            paging.Where(taskRef => taskRef.OwnedByOrganizationUnit.OrganizationId == orgId || taskRef.AccessModifier == AccessModifier.Public);

            return base.GetChildren(id, true, paging);
        }
    }
}
