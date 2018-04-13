using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Extensions;
using Core.DomainServices;
using System.Web.OData.Routing;
using Microsoft.OData;
using Microsoft.OData.UriParser;
using Ninject;
using Ninject.Extensions.Logging;
using System.Web.Http.Routing;

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