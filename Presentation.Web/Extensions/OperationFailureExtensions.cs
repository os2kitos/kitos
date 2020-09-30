using System.Net;
using Core.DomainModel.Result;

namespace Presentation.Web.Extensions
{
    public static class OperationFailureExtensions
    {
        public static HttpStatusCode ToHttpStatusCode(this OperationFailure failure)
        {
            return failure switch
            {
                OperationFailure.BadState => HttpStatusCode.BadRequest,
                OperationFailure.BadInput => HttpStatusCode.BadRequest,
                OperationFailure.NotFound => HttpStatusCode.NotFound,
                OperationFailure.Forbidden => HttpStatusCode.Forbidden,
                OperationFailure.Conflict => HttpStatusCode.Conflict,
                _ => HttpStatusCode.InternalServerError
            };
        }
    }
}