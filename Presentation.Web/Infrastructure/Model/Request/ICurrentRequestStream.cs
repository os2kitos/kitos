using System.IO;

namespace Presentation.Web.Infrastructure.Model.Request
{
    public interface ICurrentRequestStream
    {
        Stream GetInputStreamCopy();
    }
}
