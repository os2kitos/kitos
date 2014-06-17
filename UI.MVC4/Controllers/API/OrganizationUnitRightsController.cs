using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
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
        /// <param name="rights">Routing qualifier</param>
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
