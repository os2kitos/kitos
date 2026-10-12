using System.IO;
using Microsoft.AspNetCore.Http;

namespace Presentation.Web.Infrastructure.Model.Request
{
    public class CurrentRequestStream : ICurrentRequestStream
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentRequestStream(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Stream GetInputStreamCopy()
        {
            var stream = new MemoryStream();
            var requestInputStream = _httpContextAccessor.HttpContext!.Request.Body;
            requestInputStream.Position = 0;
            requestInputStream.CopyToAsync(stream).GetAwaiter().GetResult();
            requestInputStream.Position = 0;
            stream.Position = 0;
            return stream;
        }
    }
}
