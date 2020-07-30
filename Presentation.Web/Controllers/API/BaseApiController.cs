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
using Presentation.Web.Models;
using Presentation.Web.Helpers;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Presentation.Web.Infrastructure.Authorization.Controller.General;

namespace Presentation.Web.Controllers.API
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
            HttpStatusCode statusCode;
            switch (failure.FailureType)
            {
                case OperationFailure.BadInput:
                    statusCode = HttpStatusCode.BadRequest;
                    break;
                case OperationFailure.NotFound:
                    statusCode = HttpStatusCode.NotFound;
                    break;
                case OperationFailure.Forbidden:
                    statusCode = HttpStatusCode.Forbidden;
                    break;
                case OperationFailure.Conflict:
                    statusCode = HttpStatusCode.Conflict;
                    break;
                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    break;
            }

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

            //Make sure query is ordered
            query = query.OrderByField(paging.OrderBy, paging.Descending);

            //Apply post-processing
            query = paging.ApplyPostProcessing(query);

            //Load the page
            return query
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

        protected bool AllowCreate<T>(int organizationId ,IEntity entity)
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

        #endregion
    }
}
