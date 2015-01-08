using System;
using System.Collections.Generic;
using System.Net.Http;
using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class OrganizationUnitRightsController : GenericRightsController<OrganizationUnit, OrganizationRight, OrganizationRole>
    {
        private readonly IOrgUnitService _orgUnitService;

        public OrganizationUnitRightsController(IGenericRepository<OrganizationRight> rightRepository, 
            IGenericRepository<OrganizationUnit> objectRepository, IOrgUnitService orgUnitService) : base(rightRepository, objectRepository)
        {
            _orgUnitService = orgUnitService;
        }

        /// <summary>
        /// Returns all colllecteds rights for an organization unit and all sub units
        /// </summary>
        /// <param name="id">Id of the unit</param>
        /// <returns>List of rights</returns>
        public override HttpResponseMessage GetRights(int id)
        {
            try
            {
                var orgUnits = _orgUnitService.GetSubTree(id);

                var theRights = new List<OrganizationRight>();
                foreach (var orgUnit in orgUnits)
                {
                    theRights.AddRange(GetRightsQuery(orgUnit.Id));
                }

                var dtos = AutoMapper.Mapper.Map<ICollection<OrganizationRight>, ICollection<RightOutputDTO>>(theRights);

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}
