using System.Web.Http;
using System.Web.OData;
using Core.DomainServices;
using Ninject;
using Ninject.Extensions.Logging;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.ReportsControllers
{
    [Authorize]
    [ControllerEvaluationCompleted]
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