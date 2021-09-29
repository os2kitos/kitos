using System.IO;
using System.Web;

namespace Presentation.Web.Infrastructure.Model.Request
{
    public class CurrentRequestStream : ICurrentRequestStream

    {
        public Stream GetCurrentInputStream()
        {
            return HttpContext.Current.Request.InputStream;
        }
    }
}