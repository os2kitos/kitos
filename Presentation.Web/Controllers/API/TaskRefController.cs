using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class TaskRefController : GenericHierarchyApiController<TaskRef, TaskRefDTO>
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;

        public TaskRefController(IGenericRepository<TaskRef> repository, IGenericRepository<OrganizationUnit> orgUnitRepository)
            : base(repository)
        {
            _orgUnitRepository = orgUnitRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<TaskRef>(tr => _orgUnitRepository.GetByKey(tr.OwnedByOrganizationUnitId), base.GetCrudAuthorization());
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<TaskRefDTO>))]
        public HttpResponseMessage GetRootsByOrg(int orgId, bool? roots, [FromUri] PagingModel<TaskRef> paging)
        {
            Search(orgId, paging);

            return base.GetRoots(true, paging);
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<TaskRefDTO>))]
        public HttpResponseMessage GetChildrenByOrg(int id, int orgId, bool? children, [FromUri] PagingModel<TaskRef> paging)
        {
            Search(orgId, paging);

            return base.GetChildren(id, true, paging);
        }

        private static void Search(int orgId, PagingModel<TaskRef> paging)
        {
            paging.Where(taskRef => taskRef.OwnedByOrganizationUnit.OrganizationId == orgId ||
                                    taskRef.AccessModifier == AccessModifier.Public);
        }
    }
}
