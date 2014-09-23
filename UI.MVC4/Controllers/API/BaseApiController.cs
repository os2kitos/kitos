using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web.Http;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainServices;
using Ninject;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public abstract class BaseApiController : ApiController
    {
        [Inject]
        public IGenericRepository<User> UserRepository { get; set; }

        [Inject]
        public IAdminService AdminService { get; set; }

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

        protected HttpResponseMessage Ok()
        {
            return CreateResponse(HttpStatusCode.OK);
        }

        protected HttpResponseMessage Ok<T>(T response)
        {
            return CreateResponse(HttpStatusCode.OK, response);
        }
        
        protected virtual HttpResponseMessage Error<T>(T response)
        {
            if (response is SecurityException) return Unauthorized();

            return CreateResponse(HttpStatusCode.InternalServerError, response);
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

        protected HttpResponseMessage NotFound()
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
                return AdminService.IsGlobalAdmin(KitosUser);
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
#if DEBUG
                    // backdoor for Erik to publish his data
                    // TODO remove when the REST api no longer uses cookies for login
                    IEnumerable<string> header;
                    Request.Headers.TryGetValues("X-Auth", out header);
                    if (header != null && header.FirstOrDefault() == "a06b0b59-3cdf-4199-915f-f9b12c71d4c7")
                    {
                        return UserRepository.Get(u => u.Email == "erik.helweg@gmail.com").First();
                    }
                    if (header != null && header.FirstOrDefault() == "afc79f24-1a3e-40eb-8f7e-9167dce342b3")
                    {
                        return UserRepository.Get(u => u.Email == "ehl@kl.dk").First();
                    }
#endif
                    var id = Convert.ToUInt32(User.Identity.Name);
                    var user = UserRepository.Get(u => u.Id == id).FirstOrDefault();
                    if (user == null) throw new SecurityException();

                    return user;
                }
                catch (Exception)
                {
                    throw new SecurityException();
                }
            }
        }

        protected bool IsAuthenticated
        {
            get { return User.Identity.IsAuthenticated; }
        }

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
