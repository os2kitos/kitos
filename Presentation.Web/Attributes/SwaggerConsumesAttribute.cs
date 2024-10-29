using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SwaggerConsumesAttribute : Attribute
    {
        public string ContentType { get; }

        public SwaggerConsumesAttribute(string contentType)
        {
            ContentType = contentType;
        }
    }
}