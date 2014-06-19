using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Core.DomainModel;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class OrganizationUnitController : GenericHasRightsController<OrganizationUnit, OrganizationRight, OrganizationRole, OrgUnitDTO>
    {
        private readonly IOrgUnitService _orgUnitService;

        public OrganizationUnitController(IGenericRepository<OrganizationUnit> repository, IGenericRepository<OrganizationRight> rightRepository, 
            IOrgUnitService orgUnitService) 
            : base(repository, rightRepository)
        {
            _orgUnitService = orgUnitService;
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

        /// <summary>
        /// Returns every OrganizationUnit that the user can select as the default unit
        /// </summary>
        /// <param name="byUser">Routing qualifier</param>
        /// <returns></returns>
        public HttpResponseMessage GetByUser(bool? byUser)
        {
            try
            {
                var orgUnits = Repository.Get(x => x.Rights.Any(y => y.UserId == KitosUser.Id)).ToList();

                if (KitosUser.CreatedIn != null)
                {
                    var rootOrgUnit = KitosUser.CreatedIn.GetRoot();

                    orgUnits.Add(rootOrgUnit);
                }

                orgUnits = orgUnits.Distinct().ToList();

                return Ok(Map<IEnumerable<OrganizationUnit>, IEnumerable<OrgUnitSimpleDTO>>(orgUnits));
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
