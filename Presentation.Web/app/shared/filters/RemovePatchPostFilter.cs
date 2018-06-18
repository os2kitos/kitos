using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Description;

namespace Presentation.Web.app.shared.filters
{
    public class RemovePatchPostFilter: IDocumentFilter
        {
            public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
            {
            var test = swaggerDoc.paths.Where(p => p.Key.Substring(0, 7) == "/odata/" && p.Key.Contains("."));

                foreach (var path in swaggerDoc.paths.Where(p => p.Key.Substring(0, 7) == "/odata/"))
                {
                    path.Value.patch = null;
                    path.Value.post = null;
                }
            }
        }
    } 