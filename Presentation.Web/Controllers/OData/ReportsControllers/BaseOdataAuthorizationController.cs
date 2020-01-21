using System.Web.Http;
using System.Web.OData;
using Core.ApplicationServices.Authorization;
using Core.DomainServices;
using Ninject;

namespace Presentation.Web.Controllers.OData.ReportsControllers
{
    [Authorize]
    public abstract class BaseOdataAuthorizationController<T> : ODataController where T : class
    {
        protected readonly IGenericRepository<T> Repository;

        [Inject]
        public IOrganizationalUserContext UserContext { get; set; }

        [Inject]
        public IAuthorizationContext AuthorizationContext { get; set; }

        protected int UserId => UserContext.UserId;

        protected BaseOdataAuthorizationController(IGenericRepository<T> repository)
        {
            Repository = repository;
        }
    }
}