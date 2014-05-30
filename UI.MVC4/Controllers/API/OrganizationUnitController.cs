using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security;
using Core.DomainModel;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class OrganizationUnitController : GenericHasRightsController<OrganizationUnit, OrganizationRight, OrganizationRole, OrgUnitDTO>
    {
        private readonly IOrgUnitService _orgUnitService;
        private readonly IAdminService _adminService;

        public OrganizationUnitController(IGenericRepository<OrganizationUnit> repository, IGenericRepository<OrganizationRight> rightRepository, 
            IOrgUnitService orgUnitService, IAdminService adminService) 
            : base(repository, rightRepository)
        {
            _orgUnitService = orgUnitService;
            _adminService = adminService;
        }

        //TODO probably obsolete
        /*public HttpResponseMessage GetByUser(int userId)
        {
            try
            {
                var user = KitosUser;

                if(user.Id != userId) throw new SecurityException();

                var orgUnits = _orgUnitService.GetByUser(user);

                return Ok(Map<IEnumerable<OrganizationUnit>, IEnumerable<OrgUnitDTO>>(orgUnits));

            }
            catch (Exception e)
            {
                return Error(e);
            }
        }*/

        //TODO rename this into something more saying
        /// <summary>
        /// Returns every OrganizationUnit that the user has a role for
        /// </summary>
        /// <param name="userId2"></param>
        /// <returns></returns>
        public HttpResponseMessage GetByUser2(int userId2)
        {
            try
            {
                var user = KitosUser;
                if(user.Id != userId2) throw new SecurityException();

                var orgUnits = Repository.Get(x => x.Rights.Any(y => y.UserId == userId2));
                return Ok(Map<IEnumerable<OrganizationUnit>, IEnumerable<OrgUnitDTO>>(orgUnits));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetByOrganization(int organization)
        {
            try
            {
                var orgUnit = Repository.Get(o => o.OrganizationId == organization && o.Parent == null).FirstOrDefault();

                if (orgUnit == null) return NotFound();

                var item = Map<OrganizationUnit, OrgUnitDTO>(orgUnit);

                return Ok(item);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetByOrganizationFlat(int organizationId)
        {
            try
            {
                var orgUnit = Repository.Get(o => o.OrganizationId == organizationId);

                return Ok(Map(orgUnit));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public override HttpResponseMessage Patch(int id, JObject obj)
        {
            try
            {
                JToken jtoken;
                if (obj.TryGetValue("parentId", out jtoken))
                {
                    //You have to be local or global admin to change parent
                    if (!(IsGlobalAdmin() || _orgUnitService.IsLocalAdminFor(KitosUser, id)))
                        return Unauthorized();

                    var parentId = jtoken.Value<int>();
                    
                    //if the new parent is actually a descendant of the item, don't update - this would create a loop!
                    if (_orgUnitService.IsAncestorOf(parentId, id))
                    {
                        return Conflict("OrgUnit loop detected");
                    }
                }

            }
            catch (Exception e)
            {
                return Error(e);
            }
            return base.Patch(id, obj);
        }

        public override HttpResponseMessage Put(int id, OrgUnitDTO dto)
        {
            return NotAllowed();
        }

        protected override bool HasWriteAccess(OrganizationUnit obj, User user)
        {
            return _orgUnitService.HasWriteAccess(user, obj);
        }
    }
}
