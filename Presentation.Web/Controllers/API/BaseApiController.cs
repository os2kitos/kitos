using System;
using System.Collections.Generic;
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
using System.Runtime.Caching;

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
            if (response is SecurityException) return Unauthorized();

            return CreateResponse(HttpStatusCode.InternalServerError, response);
        }

        protected virtual HttpResponseMessage BadRequest()
        {
            return CreateResponse(HttpStatusCode.BadRequest);
        }

        protected virtual HttpResponseMessage Unauthorized()
        {
            return CreateResponse(HttpStatusCode.Unauthorized);
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
            return CreateResponse(HttpStatusCode.Forbidden);
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
                    var id = Convert.ToInt32(User.Identity.Name);
                    var user = UserRepository.GetByKey(id);

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

            return query.OrderByField(paging.OrderBy, paging.Descending).Skip(paging.Skip).Take(paging.Take);
        }
    }
}
