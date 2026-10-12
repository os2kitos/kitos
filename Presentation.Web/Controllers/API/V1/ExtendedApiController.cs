using System;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security;
using AutoMapper;
using Core.Abstractions.Types;
using Core.ApplicationServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Web.Extensions;
using Presentation.Web.Helpers;
using Presentation.Web.Models.API.V1;
using Serilog;

namespace Presentation.Web.Controllers.API.V1
{
    [Authorize]
    public abstract class ExtendedApiController : ControllerBase
    {
        protected ILogger Logger => Log.Logger;

        protected IMapper Mapper => HttpContext.RequestServices.GetRequiredService<IMapper>();

        protected IActionResult LogError(Exception exp, [CallerMemberName] string memberName = "")
        {
            Logger?.Error(exp, memberName);
            return Error("Der opstod en ukendt fejl. Kontakt din IT-afdeling, hvis problemet gentager sig.");
        }

        protected IActionResult CreateResponse<T>(HttpStatusCode statusCode, T response, string msg = "")
        {
            var wrap = new ApiReturnDTO<T>
            {
                Msg = msg,
                Response = response
            };
            return StatusCode((int)statusCode, wrap);
        }

        protected IActionResult CreateResponse(HttpStatusCode statusCode, string msg = "")
        {
            return CreateResponse(statusCode, new object(), msg);
        }

        protected IActionResult CreateResponse(HttpStatusCode statusCode, Exception e)
        {
            return CreateResponse(statusCode, e, e.Message);
        }

        protected IActionResult Created<T>(T response, Uri? location = null)
        {
            if (location != null)
                return base.Created(location, response);
            return StatusCode((int)HttpStatusCode.Created, new ApiReturnDTO<T> { Response = response });
        }

        protected new IActionResult Ok()
        {
            return CreateResponse(HttpStatusCode.OK);
        }

        protected IActionResult Ok<T>(T response)
        {
            return CreateResponse(HttpStatusCode.OK, response);
        }

        protected virtual IActionResult Error<T>(T response)
        {
            if (response is SecurityException)
                return Unauthorized();
            return CreateResponse(HttpStatusCode.InternalServerError, response);
        }

        protected virtual IActionResult BadRequest(string message = "")
        {
            return CreateResponse(HttpStatusCode.BadRequest, message);
        }

        protected new virtual IActionResult BadRequest(ModelStateDictionary modelState)
        {
            var errorDictionary = modelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );
            return BadRequest(string.Join("; ", errorDictionary));
        }

        protected new virtual IActionResult Unauthorized()
        {
            return CreateResponse(HttpStatusCode.Unauthorized, Constants.StatusCodeMessages.UnauthorizedErrorMessage);
        }

        protected virtual IActionResult Unauthorized<T>(T response)
        {
            return CreateResponse(HttpStatusCode.Unauthorized, response);
        }

        protected new IActionResult NoContent()
        {
            return base.NoContent();
        }

        protected new IActionResult NotFound()
        {
            return CreateResponse(HttpStatusCode.NotFound);
        }

        protected IActionResult Conflict(string msg)
        {
            return CreateResponse(HttpStatusCode.Conflict, msg);
        }

        protected IActionResult NotAllowed()
        {
            return CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        protected IActionResult Forbidden()
        {
            return CreateResponse(HttpStatusCode.Forbidden, Constants.StatusCodeMessages.ForbiddenErrorMessage);
        }

        protected IActionResult Forbidden(string msg)
        {
            return CreateResponse(HttpStatusCode.Forbidden, msg);
        }

        protected IActionResult FromOperationFailure(OperationFailure failure)
        {
            return FromOperationError(failure);
        }

        protected IActionResult FromOperationError(OperationError failure)
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
            var paginationHeader = new { TotalCount = totalCount };
            HttpContext.Response.Headers.Append("X-Pagination",
                Newtonsoft.Json.JsonConvert.SerializeObject(paginationHeader));
            return query
                .OrderByField(paging.OrderBy, paging.Descending)
                .Skip(paging.Skip)
                .Take(paging.Take);
        }
    }
}
