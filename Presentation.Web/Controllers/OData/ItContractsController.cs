﻿using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using Core.DomainModel.Organization;
using Core.ApplicationServices;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.OData;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.OData
{
    [PublicApi]
    public class ItContractsController : BaseEntityController<ItContract>
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;
        private readonly IAuthenticationService _authService;

        public ItContractsController(IGenericRepository<ItContract> repository, IGenericRepository<OrganizationUnit> orgUnitRepository, IAuthenticationService authService)
            : base(repository, authService)
        {
            _orgUnitRepository = orgUnitRepository;
            _authService = authService;
        }

        [EnableQuery]
        [ODataRoute("ItContracts")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IQueryable<ItContract>>))]
        public override IHttpActionResult Get()
        {
            var orgId = _authService.GetCurrentOrganizationId(UserId);
            var isGlobalAdmin = _authService.IsGlobalAdmin(UserId);

            return Ok(Repository.AsQueryable().Where(x => isGlobalAdmin || x.OrganizationId == orgId));
        }

        // GET /ItContracts(1)/ResponsibleOrganizationUnit
        [EnableQuery]
        [ODataRoute("ItContracts({contractKey})/ResponsibleOrganizationUnit")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<OrganizationUnit>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetResponsibleOrganizationUnit(int contractKey)
        {
            var entity = Repository.GetByKey(contractKey).ResponsibleOrganizationUnit;
            if (entity == null)
            {
                return NotFound();
            }

            if (_authService.HasReadAccess(UserId, entity))
            {
                return Ok(entity);
            }

            return Forbidden();
        }

        // GET /ItContracts(1)/ResponsibleOrganizationUnit
        [EnableQuery]
        [ODataRoute("ItContracts({contractKey})/Organization")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<Organization>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetOrganization(int contractKey)
        {
            var entity = Repository.GetByKey(contractKey).Organization;
            if (entity == null)
            {
                return NotFound();
            }

            if (_authService.HasReadAccess(UserId, entity))
            {
                return Ok(entity);
            }

            return Forbidden();
        }

        // GET /Organizations(1)/ItContracts
        [EnableQuery(MaxExpansionDepth = 3)]
        [ODataRoute("Organizations({key})/ItContracts")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IQueryable<ItContract>>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItContracts(int key)
        {
            var loggedIntoOrgId = _authService.GetCurrentOrganizationId(UserId);
            if (loggedIntoOrgId != key && !_authService.HasReadAccessOutsideContext(UserId))
            {
                return Forbidden();
            }

            //tolist requried to handle filtering on computed fields
            var result = Repository.AsQueryable().Where(m => m.OrganizationId == key);
            
            return Ok(result);
        }

        // GET /Organizations(1)/Supplier
        [EnableQuery(MaxExpansionDepth = 3)]
        [ODataRoute("Organizations({key})/Supplier")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<IQueryable<ItContract>>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetSupplier(int key)
        {
            var loggedIntoOrgId = _authService.GetCurrentOrganizationId(UserId);
            if (loggedIntoOrgId != key && !_authService.HasReadAccessOutsideContext(UserId))
            {
                return Forbidden();
            }

            var result = Repository.AsQueryable().Where(m => m.OrganizationId == key);
            return Ok(result);
        }

        // GET /Organizations(1)/ItContracts(1)
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/ItContracts({contractKey})")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<ItContract>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetItContracts(int orgKey, int contractKey)
        {
            var entity = Repository.AsQueryable().SingleOrDefault(m => m.Id == contractKey);
            if (entity == null)
            {
                return NotFound();
            }

            if (_authService.HasReadAccess(UserId, entity))
            {
                return Ok(entity);
            }

            return Forbidden();
        }

        // TODO refactor this now that we are using MS Sql Server that has support for MARS
        [EnableQuery(MaxExpansionDepth = 3)]
        [ODataRoute("Organizations({orgKey})/OrganizationUnits({unitKey})/ItContracts")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ODataResponse<List<ItContract>>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItContractsByOrgUnit(int orgKey, int unitKey)
        {
            var loggedIntoOrgId = _authService.GetCurrentOrganizationId(UserId);
            if (loggedIntoOrgId != orgKey && !_authService.HasReadAccessOutsideContext(UserId))
            {
                return Forbidden();
            }

            var contracts = new List<ItContract>();

            // using iteration instead of recursion else we're running into
            // an "multiple DataReaders open" issue and MySQL doesn't support MARS

            var queue = new Queue<int>();
            queue.Enqueue(unitKey);
            while (queue.Count > 0)
            {
                var orgUnitKey = queue.Dequeue();
                var orgUnit = _orgUnitRepository.AsQueryable()
                    .Include(x => x.Children)
                    .Include(x => x.ResponsibleForItContracts)
                    .FirstOrDefault(x => x.OrganizationId == orgKey && x.Id == orgUnitKey);

                if (orgUnit != null)
                {
                    contracts.AddRange(orgUnit.ResponsibleForItContracts);

                    var childIds = orgUnit.Children.Select(x => x.Id);
                    foreach (var childId in childIds)
                        queue.Enqueue(childId);
                }

            }
            return Ok(contracts);
        }
    }
}
