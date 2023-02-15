using System;
using System.Linq;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Microsoft.AspNet.OData;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [InternalApi]
    public class ItInterfaceExhibitsController : BaseEntityController<ItInterfaceExhibit>
    {
        private readonly IGenericRepository<ItInterface> _interfaceRepository;

        public ItInterfaceExhibitsController(
            IGenericRepository<ItInterfaceExhibit> repository,
            IGenericRepository<ItInterface> interfaceRepository)
            : base(repository)
        {
            _interfaceRepository = interfaceRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<ItInterfaceExhibit, ItInterface>(x => _interfaceRepository.GetByKey(x.Id), base.GetCrudAuthorization());
        }

        protected override IQueryable<ItInterfaceExhibit> GetAllQuery()
        {
            var accessLevel = AuthorizationContext.GetCrossOrganizationReadAccess();
            var query = Repository.AsQueryable();
            if (accessLevel < CrossOrganizationDataReadAccessLevel.All)
            {
                var orgIds = UserContext.OrganizationIds.ToList();
                query = query.Where(x =>
                    orgIds.Contains(x.ItInterface.OrganizationId) ||
                    x.ItInterface.AccessModifier == AccessModifier.Public &&
                    accessLevel == CrossOrganizationDataReadAccessLevel.Public);
            }

            return query;
        }

        [NonAction]
        public override IHttpActionResult Delete(int key) => throw new NotSupportedException();

        [NonAction]
        public override IHttpActionResult Patch(int key, Delta<ItInterfaceExhibit> delta) => throw new NotSupportedException();

        [NonAction]
        public override IHttpActionResult Post(int organizationId, ItInterfaceExhibit entity) => throw new NotSupportedException();
    }
}
