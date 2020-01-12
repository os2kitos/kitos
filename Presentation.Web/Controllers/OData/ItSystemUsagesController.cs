﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using System.Net;
using System.Net.Http;
using System.Web.Http.Routing;
using System.Web.OData.Extensions;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Core.DomainModel.Organization;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Microsoft.OData.UriParser;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.OData;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.OData
{
    [PublicApi]
    [MigratedToNewAuthorizationContext]
    public class ItSystemUsagesController : BaseEntityController<ItSystemUsage>
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;
        private readonly IGenericRepository<AccessType> _accessTypeRepository;

        public ItSystemUsagesController(
            IGenericRepository<ItSystemUsage> repository,
            IGenericRepository<OrganizationUnit> orgUnitRepository,
            IGenericRepository<AccessType> accessTypeRepository)
            : base(repository)
        {
            _orgUnitRepository = orgUnitRepository;
            _accessTypeRepository = accessTypeRepository;
        }

        /// <summary>
        /// Henter alle organisationens IT-Systemanvendelser.
        /// </summary>
        /// <param name="orgKey"></param>
        /// <returns></returns>
        [EnableQuery(MaxExpansionDepth = 4)] // MaxExpansionDepth is 4 because we need to do MainContract($expand=ItContract($expand=Supplier))
        [ODataRoute("Organizations({orgKey})/ItSystemUsages")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IEnumerable<ItSystemUsage>>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItSystems(int orgKey)
        {
            //Usages are local so full access is required
            var accessLevel = GetOrganizationReadAccessLevel(orgKey);
            if (accessLevel < OrganizationDataReadAccessLevel.All)
            {
                return Forbidden();
            }

            var result = Repository.AsQueryable().ByOrganizationId(orgKey, accessLevel);

            return Ok(result);
        }

        /// <summary>
        /// Henter alle IT-Systemanvendelser for den pågældende organisationsenhed
        /// </summary>
        /// <param name="orgKey"></param>
        /// <param name="unitKey"></param>
        /// <returns></returns>
        [EnableQuery(MaxExpansionDepth = 4)] // MaxExpansionDepth is 4 because we need to do MainContract($expand=ItContract($expand=Supplier))
        [ODataRoute("Organizations({orgKey})/OrganizationUnits({unitKey})/ItSystemUsages")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IEnumerable<ItSystemUsage>>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItSystemsByOrgUnit(int orgKey, int unitKey)
        {
            //Usages are local so full access is required
            if (GetOrganizationReadAccessLevel(orgKey) != OrganizationDataReadAccessLevel.All)
            {
                return Forbidden();
            }

            var systemUsages = new List<ItSystemUsage>();
            var queue = new Queue<int>();
            queue.Enqueue(unitKey);
            while (queue.Count > 0)
            {
                var orgUnitKey = queue.Dequeue();
                var orgUnit = _orgUnitRepository.AsQueryable()
                    .Include(x => x.Children)
                    .Include(x => x.Using.Select(y => y.ResponsibleItSystemUsage))
                    .First(x => x.OrganizationId == orgKey && x.Id == orgUnitKey);
                var responsible =
                    orgUnit.Using.Select(x => x.ResponsibleItSystemUsage).Where(x => x != null).ToList();
                systemUsages.AddRange(responsible);
                var childIds = orgUnit.Children.Select(x => x.Id);
                foreach (var childId in childIds)
                {
                    queue.Enqueue(childId);
                }
            }

            return Ok(systemUsages);
        }

        [AcceptVerbs("POST", "PUT")]
        public IHttpActionResult CreateRef([FromODataUri] int key, string navigationProperty, [FromBody] Uri link)
        {
            var itSystemUsage = Repository.GetByKey(key);
            if (itSystemUsage == null)
            {
                return NotFound();
            }

            if (!AllowWrite(itSystemUsage))
            {
                return Forbidden();
            }

            switch (navigationProperty)
            {
                case "AccessTypes":
                    var relatedKey = GetKeyFromUri<int>(Request, link);
                    var accessType = _accessTypeRepository.GetByKey(relatedKey);
                    if (accessType == null)
                    {
                        return NotFound();
                    }

                    itSystemUsage.AccessTypes.Add(accessType);
                    break;

                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }

            Repository.Save();

            return StatusCode(HttpStatusCode.NoContent);
        }

        private static TKey GetKeyFromUri<TKey>(HttpRequestMessage request, Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            var urlHelper = request.GetUrlHelper() ?? new UrlHelper(request);
            var pathHandler = (IODataPathHandler)request.GetRequestContainer().GetService(typeof(IODataPathHandler));

            string serviceRoot = urlHelper.CreateODataLink(request.ODataProperties().RouteName, pathHandler, new List<ODataPathSegment>());

            var odataPath = pathHandler.Parse(serviceRoot, uri.LocalPath, request.GetRequestContainer());

            var keySegment = odataPath.Segments.OfType<KeySegment>().FirstOrDefault();
            if (keySegment == null)
            {
                throw new InvalidOperationException("The link does not contain a key.");
            }

            var value = keySegment.Keys.FirstOrDefault().Value;
            return (TKey)value;
        }

        public IHttpActionResult DeleteRef([FromODataUri] int key, [FromODataUri] string relatedKey, string navigationProperty)
        {
            var itSystemUsage = Repository.GetByKey(key);
            if (itSystemUsage == null)
            {
                return NotFound();
            }

            if (!AllowWrite(itSystemUsage))
            {
                return Forbidden();
            }

            switch (navigationProperty)
            {
                case "AccessTypes":
                    var accessTypeId = Convert.ToInt32(relatedKey);
                    var accessType = _accessTypeRepository.GetByKey(accessTypeId);

                    if (accessType == null)
                    {
                        return NotFound();
                    }
                    itSystemUsage.AccessTypes.Remove(accessType);
                    break;
                default:
                    return StatusCode(HttpStatusCode.NotImplemented);

            }

            Repository.Save();

            return StatusCode(HttpStatusCode.NoContent);
        }

    }
}
