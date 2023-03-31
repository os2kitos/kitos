using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security;
using System.Web.Http;
using AutoMapper;
using Core.ApplicationServices;
using Ninject;
using Ninject.Extensions.Logging;
using Presentation.Web.Extensions;
using Presentation.Web.Helpers;
using Presentation.Web.Models.API.V1;
using System.Web.Http.ModelBinding;
using Core.Abstractions.Types;

namespace Presentation.Web.Controllers.API.V1
{
    [Authorize]
    public abstract class ExtendedApiController : ApiController
    {
        [Inject]
        public ILogger Logger { get; set; }

        [Inject]
        public IMapper Mapper { get; set; }

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
            var errorDictionary = modelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );

            return BadRequest(string.Join("; ", errorDictionary));
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
            return Request.CreateResponse(HttpStatusCode.NoContent);
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
    }
}
