using System.IO;
using System.Web;

namespace Presentation.Web.Infrastructure.Model.Request
{
    public class CurrentRequestStream : ICurrentRequestStream
    {
        public Stream GetInputStreamCopy()
        {
            var stream = new MemoryStream();
            var requestInputStream = HttpContext.Current.Request.InputStream;
            requestInputStream.Position = 0;
            requestInputStream.CopyTo(stream);
            requestInputStream.Position = 0;
            stream.Position = 0;
            return stream;
        }
    }
}