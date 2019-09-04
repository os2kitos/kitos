using System.Net;

namespace Core.DomainModel
{
    public enum ReturnType
    {
        Ok,

        NotFound,

        Forbidden,

        Error
    }

    public class Return<T>
    {
        public ReturnType ReturnCode;

        public T ReturnValue;
    }
}
