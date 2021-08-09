using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security;
using System.Web.Http;
using AutoMapper;
using Core.ApplicationServices;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.DomainModel;
using Core.DomainModel.Result;
using Core.DomainServices.Authorization;
using Ninject;
using Ninject.Extensions.Logging;
using Presentation.Web.Extensions;
using Presentation.Web.Models;
using Presentation.Web.Helpers;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Presentation.Web.Infrastructure.Authorization.Controller.General;
using Presentation.Web.Models.API.V1;
using System.Web.Http.ModelBinding;

namespace Presentation.Web.Controllers.API.V1
{
    [Authorize]
    public abstract class BaseApiController : ApiController
    {
        [Inject]
        public ILogger Logger { get; set; }

        [Inject]
        public IOrganizationalUserContext UserContext { get; set; }

        [Inject]
        public IAuthorizationContext AuthorizationContext { get; set; }

        [Inject]
        public IMapper Mapper { get; set; }

        //Lazy to make sure auth service is available when resolved
        private readonly Lazy<IControllerAuthorizationStrategy> _authorizationStrategy;
        private readonly Lazy<IControllerCrudAuthorization> _crudAuthorization;

        protected IControllerAuthorizationStrategy AuthorizationStrategy => _authorizationStrategy.Value;

        protected IControllerCrudAuthorization CrudAuthorization => _crudAuthorization.Value;

        protected BaseApiController()
        {
            _authorizationStrategy = new Lazy<IControllerAuthorizationStrategy>(() => new ContextBasedAuthorizationStrategy(AuthorizationContext));
            _crudAuthorization = new Lazy<IControllerCrudAuthorization>(GetCrudAuthorization);
        }

        protected HttpResponseMessage LogError(Exception exp, [CallerMemberName] string memberName = "")
        {
            Logger?.Error(exp, memberName);

            return Error("Der opstod en ukendt fejl. Kontakt din IT-afdeling, hvis problemet gentager sig.");
        }

        protected HttpResponseMessage CreateResponse<T>(HttpStatusCode statusCode, T response, string msg = "")
        {
            var wrap = new ApiReturnDTO<T>
            {
                Msg = msg,
                Response = response
            };

            return Request.CreateResponse(statusCode, wrap);
        }

        protected HttpResponseMessage CreateResponse(HttpStatusCode statusCode, string msg = "")
        {
            return CreateResponse(statusCode, new object(), msg);
        }

        protected HttpResponseMessage CreateResponse(HttpStatusCode statusCode, Exception e)
        {
            return CreateResponse(statusCode, e, e.Message);
        }

        protected HttpResponseMessage Created<T>(T response, Uri location = null)
        {
            var result = CreateResponse(HttpStatusCode.Created, response);
            if (location != null)
                result.Headers.Location = location;

            return result;
        }

        protected new HttpResponseMessage Ok()
        {
            return CreateResponse(HttpStatusCode.OK);
        }

        protected new HttpResponseMessage Ok<T>(T response)
        {
            return CreateResponse(HttpStatusCode.OK, response);
        }

        protected virtual HttpResponseMessage Error<T>(T response)
        {
            if (response is SecurityException)
            {
                return Unauthorized();
            }

            return CreateResponse(HttpStatusCode.InternalServerError, response);
        }

        protected new virtual HttpResponseMessage BadRequest(string message = "")
        {
            return CreateResponse(HttpStatusCode.BadRequest, message);
        }

        protected new virtual HttpResponseMessage BadRequest(ModelStateDictionary modelState)
        {
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, modelState);
        }

        protected virtual HttpResponseMessage Unauthorized()
        {
            return CreateResponse(HttpStatusCode.Unauthorized, Constants.StatusCodeMessages.UnauthorizedErrorMessage);
        }

        protected virtual HttpResponseMessage Unauthorized<T>(T response)
        {
            return CreateResponse(HttpStatusCode.Unauthorized, response);
        }

        protected HttpResponseMessage NoContent()
        {
            return CreateResponse(HttpStatusCode.NoContent);
        }

        protected new HttpResponseMessage NotFound()
        {
            return CreateResponse(HttpStatusCode.NotFound);
        }

        protected HttpResponseMessage Conflict(string msg)
        {
            return CreateResponse(HttpStatusCode.Conflict, msg);
        }

        protected HttpResponseMessage NotAllowed()
        {
            return CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        protected HttpResponseMessage Forbidden()
        {
            return CreateResponse(HttpStatusCode.Forbidden, Constants.StatusCodeMessages.ForbiddenErrorMessage);
        }

        protected HttpResponseMessage Forbidden(string msg)
        {
            return CreateResponse(HttpStatusCode.Forbidden, msg);
        }

        protected HttpResponseMessage FromOperationFailure(OperationFailure failure)
        {
            return FromOperationError(failure);
        }

        protected HttpResponseMessage FromOperationError(OperationError failure)
        {
            var statusCode = failure.FailureType.ToHttpStatusCode();

            return CreateResponse(statusCode, failure.Message.GetValueOrFallback(string.Empty));
        }

        protected int UserId => UserContext.UserId;

        protected virtual TDest Map<TSource, TDest>(TSource item)
        {
            return Mapper.Map<TDest>(item);
        }

        protected virtual IQueryable<T> Page<T>(IQueryable<T> query, PagingModel<T> paging)
        {
            query = paging.Filter(query);

            var totalCount = query.Count();
            var paginationHeader = new
            {
                TotalCount = totalCount
            };
            System.Web.HttpContext.Current.Response.Headers.Add("X-Pagination",
                                                                Newtonsoft.Json.JsonConvert.SerializeObject(
                                                                    paginationHeader));

            //Load the page
            return query
                .OrderByField(paging.OrderBy, paging.Descending)
                .Skip(paging.Skip)
                .Take(paging.Take);
        }

        #region access control

        protected virtual IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new RootEntityCrudAuthorization(AuthorizationStrategy);
        }

        protected EntityReadAccessLevel GetEntityTypeReadAccessLevel<T>()
        {
            return AuthorizationStrategy.GetEntityTypeReadAccessLevel<T>();
        }

        protected CrossOrganizationDataReadAccessLevel GetCrossOrganizationReadAccessLevel()
        {
            return AuthorizationStrategy.GetCrossOrganizationReadAccess();
        }

        protected OrganizationDataReadAccessLevel GetOrganizationReadAccessLevel(int organizationId)
        {
            return AuthorizationStrategy.GetOrganizationReadAccessLevel(organizationId);
        }

        protected bool AllowRead(IEntity entity)
        {
            return CrudAuthorization.AllowRead(entity);
        }

        protected bool AllowModify(IEntity entity)
        {
            return CrudAuthorization.AllowModify(entity);
        }

        protected virtual bool AllowCreate<T>(int organizationId, IEntity entity)
        {
            return CrudAuthorization.AllowCreate<T>(organizationId, entity);
        }

        protected bool AllowCreate<T>(int organizationId)
        {
            return AuthorizationStrategy.AllowCreate<T>(organizationId);
        }

        protected bool AllowDelete(IEntity entity)
        {
            return CrudAuthorization.AllowDelete(entity);
        }

        protected bool AllowEntityVisibilityControl(IEntity entity)
        {
            return AuthorizationStrategy.HasPermission(new VisibilityControlPermission(entity));
        }

        protected virtual IEntity GetEntity(int id) => throw new NotSupportedException("This endpoint does not support access rights");

        protected virtual bool AllowCreateNewEntity(int organizationId) => throw new NotSupportedException("This endpoint does not support generic creation rights");

        [HttpGet]
        [InternalApi]
        /// <summary>
        /// GET api/T/GetAccessRights
        /// Checks what access rights the user has for the given entities
        /// </summary>
        public virtual HttpResponseMessage GetAccessRights(bool? getEntitiesAccessRights, int organizationId)
        {
            if (GetOrganizationReadAccessLevel(organizationId) == OrganizationDataReadAccessLevel.None)
            {
                return Forbidden();
            }
            return Ok(new EntitiesAccessRightsDTO
            {
                CanCreate = AllowCreateNewEntity(organizationId),
                CanView = true
            });
        }

        [HttpGet]
        [InternalApi]
        /// <summary>
        /// GET api/T/id?GetAccessRightsForEntity
        /// Checks what access rights the user has for the given entity
        /// </summary>
        /// <param name="id">The id of the object</param>
        public virtual HttpResponseMessage GetAccessRightsForEntity(int id, bool? getEntityAccessRights)
        {
            var item = GetEntity(id);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(GetAccessRightsForEntity(item));
        }

        private EntityAccessRightsDTO GetAccessRightsForEntity(IEntity item)
        {
            return new EntityAccessRightsDTO
            {
                Id = item.Id,
                CanDelete = AllowDelete(item),
                CanEdit = AllowModify(item),
                CanView = AllowRead(item)
            };
        }

        [HttpPost]
        [InternalApi]
        /// <summary>
        /// POST api/T/idListAsCsv?getEntityListAccessRights
        /// Uses POST verb to allow use of body for potentially long list of ids
        /// Checks what access rights the user has for the given entities identified by the <see cref=""/> list
        /// </summary>
        /// <param name="ids">The ids of the objects</param>
        public virtual HttpResponseMessage PostSearchAccessRightsForEntityList([FromBody] int[] ids, bool? getEntityListAccessRights)
        {
            if (ids == null || ids.Length == 0)
            {
                return BadRequest();
            }

            return Ok(
                ids
                    .Distinct()
                    .Select(GetEntity)
                    .Where(entity => entity != null)
                    .Select(GetAccessRightsForEntity)
                    .ToList()
            );
        }

        #endregion
    }
}
