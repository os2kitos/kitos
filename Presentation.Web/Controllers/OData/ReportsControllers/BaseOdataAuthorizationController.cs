using System.Web.Http;
using System.Web.OData;
using Core.DomainServices;
using Ninject;
using Ninject.Extensions.Logging;

namespace Presentation.Web.Controllers.OData.ReportsControllers
{
    [Authorize]
    public abstract class BaseOdataAuthorizationController<T> : ODataController where T : class
    {
        protected readonly IGenericRepository<T> Repository;
        [Inject]
        public ILogger Logger { get; set; }
        protected int UserId
        {
            get
            {
                int userId;
                int.TryParse(User.Identity.Name, out userId);
                return userId;
            }
        }
        protected BaseOdataAuthorizationController(IGenericRepository<T> repository)
        {
            Repository = repository;
        }
    }
}