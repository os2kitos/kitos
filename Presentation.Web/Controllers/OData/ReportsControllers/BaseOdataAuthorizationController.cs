using System.Web.Http;
using System.Web.OData;
using Core.ApplicationServices.Authorization;
using Core.DomainServices;
using Ninject;
using Ninject.Extensions.Logging;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.ReportsControllers
{
    [Authorize]
    [MigratedToNewAuthorizationContext]
    public abstract class BaseOdataAuthorizationController<T> : ODataController where T : class
    {
        protected readonly IGenericRepository<T> Repository;

        [Inject]
        public ILogger Logger { get; set; }

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