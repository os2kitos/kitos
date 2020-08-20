using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using System.Net;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Core.DomainModel.Organization;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.OData;
using Swashbuckle.Swagger.Annotations;
using Infrastructure.DataAccess;

namespace Presentation.Web.Controllers.OData.Helpers
{
    public class OrganizationUnitsHelper
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;
        private readonly IGenericRepository<AccessType> _accessTypeRepository;

        public OrganizationUnitsHelper(
            IGenericRepository<ItContract> repository,
            IGenericRepository<OrganizationUnit> orgUnitRepository)
        {
            _orgUnitRepository = orgUnitRepository;
        }

        public OrganizationUnitsHelper(
            IGenericRepository<ItSystemUsage> repository,
            IGenericRepository<OrganizationUnit> orgUnitRepository,
            IGenericRepository<AccessType> accessTypeRepository)
        {
            _orgUnitRepository = orgUnitRepository;
            _accessTypeRepository = accessTypeRepository;
        }



        public GetItSystemsByOrgUnit(int unitKey, int orgKey, IGenericRepository<T> _repository)
        {
            var orgUnitTreeIds = new List<int>();
            var queue = new Queue<int>();
            queue.Enqueue(unitKey);
            while (queue.Count > 0)
            {
                var orgUnitKey = queue.Dequeue();
                orgUnitTreeIds.Add(orgUnitKey);
                var orgUnit = _repository.AsQueryable()
                    .Include(x => x.Children)
                    .First(x => x.OrganizationId == orgKey && x.Id == orgUnitKey);

                //Add sub tree
                var childIds = orgUnit.Children.Select(x => x.Id);
                foreach (var childId in childIds)
                {
                    queue.Enqueue(childId);
                }
            }

            var result = Repository
                .AsQueryable()
                .ByOrganizationId(orgKey)
                .Where(usage => usage.ResponsibleUsage != null && orgUnitTreeIds.Contains(usage.ResponsibleUsage.OrganizationUnitId));

            return result;
        }






    }
}