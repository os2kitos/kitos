﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security;
using System.Web.Http;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainServices;
using Ninject;
using Ninject.Extensions.Logging;
using Presentation.Web.Models;
using Presentation.Web.Helpers;
using Presentation.Web.Infrastructure.Authorization.Context;
using Presentation.Web.Infrastructure.Authorization.Controller;

namespace Presentation.Web.Controllers.API
{
    [Authorize]
    public abstract class BaseApiController : ApiController
    {
        [Inject]
        public IGenericRepository<User> UserRepository { get; set; }

        [Inject]
        public IAuthenticationService AuthenticationService { get; set; }

        [Inject]
        public IFeatureChecker FeatureChecker { get; set; }

        [Inject]
        public ILogger Logger { get; set; }

        //Lazy to make sure auth service is available when resolved
        private readonly Lazy<IControllerAuthorizationStrategy> _authorizationStrategy;

        protected IControllerAuthorizationStrategy AuthorizationStrategy => _authorizationStrategy.Value;

        protected BaseApiController(IAuthorizationContext authorizationContext = null)
        {
            _authorizationStrategy = new Lazy<IControllerAuthorizationStrategy>(() =>

                authorizationContext == null
                    ? (IControllerAuthorizationStrategy)new LegacyAuthorizationStrategy(AuthenticationService, () => UserId)
                    : new ContextBasedAuthorizationStrategy(authorizationContext)
            );
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

        protected virtual HttpResponseMessage BadRequest()
        {
            return CreateResponse(HttpStatusCode.BadRequest);
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


        protected bool IsGlobalAdmin()
        {
            try
            {
                int userId;
                int.TryParse(User.Identity.Name, out userId);

                return AuthenticationService.IsGlobalAdmin(userId);
            }
            catch
            {
                return false;
            }
        }

        protected User KitosUser
        {
            get
            {
                try
                {
                    var user = UserRepository.GetByKey(UserId);

                    if (user == null)
                        throw new SecurityException();

                    return user;
                }
                catch (Exception exp)
                {
                    Logger?.Error("Error in property KitosUser", exp);
                    throw new SecurityException();
                }
            }
        }

        protected int UserId => Convert.ToInt32(User.Identity.Name);

        protected bool IsAuthenticated => User.Identity.IsAuthenticated;

        protected virtual TDest Map<TSource, TDest>(TSource item)
        {
            return AutoMapper.Mapper.Map<TDest>(item);
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

        protected bool AllowOrganizationReadAccess(int organizationId)
        {
            return AuthorizationStrategy.AllowOrganizationReadAccess(organizationId);
        }

        protected bool AllowRead(IEntity entity)
        {
            return AuthorizationStrategy.AllowRead(entity);
        }

        protected bool AllowModify(IEntity entity)
        {
            return AuthorizationStrategy.AllowModify(entity);
        }

        protected bool AllowCreate<T>(IEntity entity)
        {
            return AuthorizationStrategy.AllowCreate<T>(entity);
        }

        protected bool AllowCreate<T>()
        {
            return AuthorizationStrategy.AllowCreate<T>();
        }

        protected bool AllowDelete(IEntity entity)
        {
            return AuthorizationStrategy.AllowDelete(entity);
        }

        protected bool AllowEntityVisibilityControl(IEntity entity)
        {
            return AuthorizationStrategy.AllowEntityVisibilityControl(entity);
        }
       
        #endregion
    }
}
