using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using System.Net;
using System.Security.Principal;
using Core.ApplicationServices;
using Core.DomainModel.Organization;
using Presentation.Web.Access;

namespace Presentation.Web.Controllers.OData
{
    public interface IOrganizationContextFactory
    {
        OrganizationContext CreateOrganizationContext(int organizationId);
    }

    class OrganizationContextFactory : IOrganizationContextFactory
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;

        public OrganizationContextFactory(IGenericRepository<User> userRepository, IGenericRepository<Organization> organizationRepository)
        {
            _userRepository = userRepository;
            _organizationRepository = organizationRepository;
        }

        public OrganizationContext CreateOrganizationContext(int organizationId)
        {
            return new OrganizationContext(_userRepository, _organizationRepository, organizationId);
        }
    }

    public partial class ItSystemsController : BaseEntityController<ItSystem>
    {
        private readonly IAuthenticationService _authService;
        private readonly IOrganizationContextFactory _contextFactory;

        public ItSystemsController(IGenericRepository<ItSystem> repository, IAuthenticationService authService, IOrganizationContextFactory contextFactory)
            : base(repository, authService)
        {
            _authService = authService;
            _contextFactory = contextFactory;
        }

        // GET /Organizations(1)/ItSystems
        [EnableQuery]
        [ODataRoute("Organizations({key})/ItSystems")]
        public IHttpActionResult GetItSystems(int key)
        {
            var loggedIntoOrgId = _authService.GetCurrentOrganizationId(UserId);

            var organizationContext = _contextFactory.CreateOrganizationContext(loggedIntoOrgId);
            if (!organizationContext.AllowReads(UserId))
            { 
                return StatusCode(HttpStatusCode.Forbidden);
            }

            var result = Repository.AsQueryable().Where(m => m.OrganizationId == key || m.AccessModifier == AccessModifier.Public);

            return Ok(result);
        }

        [ODataRoute("ItSystems")]
        public override IHttpActionResult Get()
        {
            return base.Get();
        }

        // GET /Organizations(1)/BelongingSystems
        [EnableQuery]
        [ODataRoute("Organizations({key})/BelongingSystems")]
        public IHttpActionResult GetBelongingSystems(int key)
        {
            var loggedIntoOrgId = _authService.GetCurrentOrganizationId(UserId);
            if (!_authService.HasReadAccessOutsideContext(UserId))
            {
                if (loggedIntoOrgId != key)
                    return StatusCode(HttpStatusCode.Forbidden);

                var result = Repository.AsQueryable().Where(m => m.BelongsToId == key);
                return Ok(result);
            }
            else
            {
                var result = Repository.AsQueryable().Where(m => m.OrganizationId == key || m.AccessModifier == AccessModifier.Public);
                return Ok(result);
            }
        }

        // GET /Organizations(1)/ItSystems(1)
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/ItSystems({sysKey})")]
        public IHttpActionResult GetItSystems(int orgKey, int sysKey)
        {
            var entity = Repository.AsQueryable().SingleOrDefault(m => m.Id == sysKey);
            if (entity == null)
                return NotFound();

            if (_authService.HasReadAccess(UserId, entity))
                return Ok(entity);

            return StatusCode(HttpStatusCode.Forbidden);
        }
    }
}
