using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class TaskRefController : GenericHierarchyApiController<TaskRef, TaskRefDTO>
    {
        public TaskRefController(IGenericRepository<TaskRef> repository)
            : base(repository)
        {
        }

        public HttpResponseMessage GetRootsByOrgUnit(int orgUnitId, bool? roots, [FromUri] PagingModel<TaskRef> paging)
        {
            paging.Where(taskRef => taskRef.OwnedByOrganizationUnitId == orgUnitId || taskRef.AccessModifier == AccessModifier.Public);

            return base.GetRoots(true, paging);
        }

        public HttpResponseMessage GetChildrenByOrgUnit(int id, int orgUnitId, bool? children, [FromUri] PagingModel<TaskRef> paging)
        {
            paging.Where(taskRef => taskRef.OwnedByOrganizationUnitId == orgUnitId || taskRef.AccessModifier == AccessModifier.Public);

            return base.GetChildren(id, true, paging);
        }

        public HttpResponseMessage GetRootsByOrg(int orgId, bool? roots, [FromUri] PagingModel<TaskRef> paging)
        {
            paging.Where(taskRef => taskRef.OwnedByOrganizationUnit.OrganizationId == orgId || taskRef.AccessModifier == AccessModifier.Public);

            return base.GetRoots(true, paging);
        }

        public HttpResponseMessage GetChildrenByOrg(int id, int orgId, bool? children, [FromUri] PagingModel<TaskRef> paging)
        {
            paging.Where(taskRef => taskRef.OwnedByOrganizationUnit.OrganizationId == orgId || taskRef.AccessModifier == AccessModifier.Public);

            return base.GetChildren(id, true, paging);
        }
    }
}
