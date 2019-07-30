using System;
using System.Web.Http.Description;

namespace Presentation.Web.Infrastructure.Attributes
{
    [ApiExplorerSettings(IgnoreApi = false)]
    public class PublicApiAttribute : Attribute
    {
    }
}