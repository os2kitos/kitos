using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http.Description;

namespace Presentation.Web.app.shared.filters
{
    public class RemovePatchPostFilter: IDocumentFilter
        {
            public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
            {
                var optionsCtrls = from t in Assembly.GetExecutingAssembly().GetTypes()
                    where t.IsClass && (t.Namespace == "Presentation.Web.Controllers.OData.OptionControllers" || t.Namespace == "Presentation.Web.Controllers.OData.AttachedOptions")
                    select t;
                var modifyDocs = swaggerDoc;
                foreach (var path in modifyDocs.paths.Where(p => p.Key.Substring(0, 7) == "/odata/"))
                {
                    //if (!path.Key.Contains("/odata/Organizations"))
                    //{
                    //    path.Value.delete = null;
                    //    path.Value.get = null;
                    //    path.Value.head = null;
                    //    path.Value.options = null;
                    //    path.Value.put = null;
                    //    path.Value.patch = null;
                    //    path.Value.post = null;
                    //}
                    //else
                    //{
                    //    path.Value.patch = null;
                    //    path.Value.post = null;
                    //}
                string subkey = "";
                if (path.Key.Contains("{Id}"))
                {
                    var value = (path.Key.Length - 13);
                    subkey = path.Key.Substring(7, value);
                }
                else
                {
                    subkey = path.Key.Substring(7);
                }
                if (path.Key.Substring(0, 12) == "/odata/Local" || optionsCtrls.Any(a => a.Name.Contains(subkey)))
                {
                    path.Value.delete = null;
                    path.Value.get = null;
                    path.Value.head = null;
                    path.Value.options = null;
                    path.Value.put = null;
                    path.Value.patch = null;
                    path.Value.post = null;
                }
                else
                {
                    path.Value.patch = null;
                    path.Value.post = null;
                }
            }
        }
        }
    } 