﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Core.DomainModel.Events;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1;

namespace Presentation.Web.Controllers.API.V1
{
    [InternalApi]
    public class OrganizationUnitRightController : GenericRightsController<OrganizationUnit, OrganizationUnitRight, OrganizationUnitRole>
    {
        private readonly IOrgUnitService _orgUnitService;

        public OrganizationUnitRightController(
            IGenericRepository<OrganizationUnitRight> rightRepository,
            IGenericRepository<OrganizationUnit> objectRepository,
            IDomainEvents domainEvents,
            IOrgUnitService orgUnitService)
            : base(rightRepository, objectRepository, domainEvents)
        {
            _orgUnitService = orgUnitService;
        }

        /// <summary>
        /// Returns all rights for an organization unit and all sub units
        /// </summary>
        /// <param name="id">Id of the unit</param>
        /// <param name="paged"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns>List of rights</returns>
        public HttpResponseMessage GetRights(int id, bool? paged, int skip = 0, int take = 50)
        {
            try
            {
                var theRights = GetOrganizationRights(id);

                var paginationHeader = new
                {
                    TotalCount = theRights.Count()
                };
                System.Web.HttpContext.Current.Response.Headers.Add("X-Pagination",
                                                                    Newtonsoft.Json.JsonConvert.SerializeObject(
                                                                        paginationHeader));

                var pagedRights = theRights.Skip(skip).Take(take).ToList();

                var dtos = Map<ICollection<OrganizationUnitRight>, ICollection<RightOutputDTO>>(pagedRights);

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        private List<OrganizationUnitRight> GetOrganizationRights(int id)
        {
            var orgUnits = _orgUnitService.GetSubTree(id);

            var theRights = new List<OrganizationUnitRight>();
            foreach (var orgUnit in orgUnits)
            {
                theRights.AddRange(GetRightsQuery(orgUnit.Id));
            }
            return theRights.Where(AllowRead).ToList();
        }
    }
}
