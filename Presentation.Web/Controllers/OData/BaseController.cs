using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Extensions;
using Core.DomainServices;
using System.Web.OData.Routing;
using Microsoft.OData.UriParser;
using Ninject;
using Ninject.Extensions.Logging;
using System.Web.Http.Routing;

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
            var pathHandler = (IODataPathHandler)request.GetRequestContainer().GetService(typeof(IODataPathHandler));

            string serviceRoot = urlHelper.CreateODataLink(request.ODataProperties().RouteName,pathHandler, new List<ODataPathSegment>());

            var odataPath = pathHandler.Parse(serviceRoot,uri.LocalPath, request.GetRequestContainer());

            var keySegment = odataPath.Segments.OfType<KeySegment>().FirstOrDefault();
            if (keySegment == null)
            {
                throw new InvalidOperationException("The link does not contain a key.");
            }

            var value = keySegment.Keys.FirstOrDefault().Value;
            return (TKey)value;
        }

        protected virtual IHttpActionResult Forbidden()
        {
            return ResponseMessage(new HttpResponseMessage(HttpStatusCode.Forbidden));
        }

    }
}
