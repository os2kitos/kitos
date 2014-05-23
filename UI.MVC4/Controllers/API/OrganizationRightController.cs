using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class OrganizationRightController : GenericRightController<OrganizationRight, OrganizationUnit, OrganizationRole>
    {
        private readonly IOrgUnitService _orgUnitService;

        public OrganizationRightController(IGenericRepository<OrganizationRight> repository, IGenericRepository<OrganizationUnit> orgUnitRepository, IOrgUnitService orgUnitService) 
            : base(repository, orgUnitRepository)
        {
            _orgUnitService = orgUnitService;
        }

        protected override bool HasWriteAccess(int objId, User user)
        {
            return _orgUnitService.HasWriteAccess(user, objId);
        }

        /* returns the organisations-rights for an organization unit and all units in the subtree */
        public HttpResponseMessage GetSubTreeRights(int organizationUnitId)
        {
            try
            {
                var orgUnits = _orgUnitService.GetSubTree(organizationUnitId);

                var rights = new List<OrganizationRight>();
                foreach (var orgUnit in orgUnits)
                {
                    rights.AddRange(GetAll(orgUnit.Id));
                }

                var dtos = AutoMapper.Mapper.Map<ICollection<OrganizationRight>, ICollection<RightOutputDTO>>(rights);

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}
