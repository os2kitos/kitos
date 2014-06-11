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

        /// <summary>
        /// Returns all colllecteds rights for an organization unit and all sub units
        /// </summary>
        /// <param name="id">Id of the unit</param>
        /// <param name="rights">Routing qualifier</param>
        /// <returns>List of rights</returns>
        public override HttpResponseMessage GetRights(int id, bool? rights)
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

        //TODO probably don't use this, use get by organization instead
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

        /// <summary>
        /// Returns every OrganizationUnit that the user has a role for
        /// </summary>
        /// <param name="byUser">Routing qualifier</param>
        /// <returns></returns>
        public HttpResponseMessage GetByUser(bool? byUser)
        {
            try
            {
                var orgUnits = Repository.Get(x => x.Rights.Any(y => y.UserId == KitosUser.Id));
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
                    //TODO: You have to be local or global admin to change parent

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

        public override HttpResponseMessage Put(int id, JObject jObject)
        {
            return NotAllowed();
        }
    }
}
