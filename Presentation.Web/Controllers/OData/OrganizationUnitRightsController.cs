using Core.DomainServices;
using Core.DomainModel.Organization;
using System.Web.OData;
using System.Web.OData.Routing;
using System.Web.Http;
using System.Linq;
using System;
using System.Net;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;

namespace Presentation.Web.Controllers.OData
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
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
            return new ChildEntityCrudAuthorization<OrganizationUnitRight>(or => _orgUnitRepository.GetByKey(or.ObjectId), base.GetCrudAuthorization());
        }

        // GET /Users(1)/ItContractRights
        [EnableQuery]
        [ODataRoute("Users({userId})/OrganizationUnitRights")]
        public IHttpActionResult GetByUser(int userId)
        {
            // TODO figure out how to check auth
            var result = Repository.AsQueryable().Where(x => x.UserId == userId);
            return Ok(result);
        }

        public override IHttpActionResult Delete(int key)
        {
            var entity = Repository.GetByKey(key);

            if (entity == null)
            {
                return NotFound();
            }

            if (!AllowDelete(entity))
            {
                return Unauthorized();
            }

            try
            {
                Repository.DeleteByKey(key);
                Repository.Save();
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        public override IHttpActionResult Patch(int key, Delta<OrganizationUnitRight> delta)
        {
            var entity = Repository.GetByKey(key);

            // does the entity exist?
            if (entity == null)
            {
                return NotFound();
            }

            // check if user is allowed to write to the entity
            if (!AllowWrite(entity))
            {
                return Forbidden();
            }

            // check model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // patch the entity
                delta.Patch(entity);
                Repository.Save();
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }

            // add the request header "Prefer: return=representation"
            // if you want the updated entity returned,
            // else you'll just get 204 (No Content) returned
            return Updated(entity);
        }
    }
}
