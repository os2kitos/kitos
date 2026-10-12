using System.Linq;
using System.Net;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.DomainServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Web.Extensions;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)]
    public abstract class BaseController<T> : ODataController where T : class
    {
        protected readonly IGenericRepository<T> Repository;

        protected IOrganizationalUserContext UserContext => HttpContext.RequestServices.GetRequiredService<IOrganizationalUserContext>();
        protected IAuthorizationContext AuthorizationContext => HttpContext.RequestServices.GetRequiredService<IAuthorizationContext>();

        protected BaseController(IGenericRepository<T> repository)
        {
            Repository = repository;
        }

        protected int UserId => UserContext.UserId;

        public virtual IActionResult Get()
        {
            return Ok(Repository.AsQueryable().ToList());
        }

        public virtual IActionResult Get(int key)
        {
            var entity = Repository.GetByKey(key);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        protected IActionResult Forbidden()
        {
            return StatusCode((int)HttpStatusCode.Forbidden);
        }
    }
}
