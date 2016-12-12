using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Extensions;
using Core.DomainServices;
using System.Web.OData.Routing;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Ninject;
using Ninject.Extensions.Logging;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Routing;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;

namespace Presentation.Web.Controllers.OData
{
    [Authorize]
    public abstract class BaseController<T> : ODataController where T : class
    {
        protected readonly IGenericRepository<T> Repository;

        [Inject]
        public ILogger Logger { get; set; }

        protected BaseController(IGenericRepository<T> repository)
        {
            Repository = repository;
        }

        protected int UserId
        {
            get
            {
                int userId;
                int.TryParse(User.Identity.Name, out userId);
                return userId;
            }
        }

        [EnableQuery]
        public virtual IHttpActionResult Get()
        {
            return Ok(Repository.AsQueryable());
        }

        [EnableQuery(MaxExpansionDepth = 4)]
        public virtual IHttpActionResult Get(int key)
        {
            var entity = Repository.GetByKey(key);
            return Ok(entity);
        }


        protected TKey GetKeyFromUri<TKey>(HttpRequestMessage request, Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            var urlHelper = request.GetUrlHelper() ?? new UrlHelper(request);

            string serviceRoot = urlHelper.CreateODataLink(
                request.ODataProperties().RouteName,
                request.ODataProperties().PathHandler, new List<ODataPathSegment>());
            var odataPath = request.ODataProperties().PathHandler.Parse(
                request.ODataProperties().Model,
                serviceRoot, uri.LocalPath);

            var keySegment = odataPath.Segments.OfType<KeyValuePathSegment>().FirstOrDefault();
            if (keySegment == null)
            {
                throw new InvalidOperationException("The link does not contain a key.");
            }

            var value = ODataUriUtils.ConvertFromUriLiteral(keySegment.Value, ODataVersion.V4);
            return (TKey)value;
        }
    }
}
