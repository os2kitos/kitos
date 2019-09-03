using System.Net;

namespace Core.DomainModel
{
    public class Return<T>
    {
        public HttpStatusCode StatusCode;

        public T ReturnValue;
    }
}
