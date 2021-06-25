using System.Web.Http;
using AutoMapper;
using Microsoft.AspNet.OData;
using Core.ApplicationServices.Authorization;
using Core.DomainServices;
using Ninject;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.ReportsControllers
{
    [Authorize]
    [DenyRightsHoldersAccess]
    public abstract class BaseOdataAuthorizationController<T> : ODataController where T : class
    {
        protected readonly IGenericRepository<T> Repository;

        [Inject]
        public IOrganizationalUserContext UserContext { get; set; }

        [Inject]
        public IAuthorizationContext AuthorizationContext { get; set; }

        [Inject]
        public IMapper Mapper { get; set; }

        protected int UserId => UserContext.UserId;

        protected BaseOdataAuthorizationController(IGenericRepository<T> repository)
        {
            Repository = repository;
        }
    }
}