using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItProjectController : GenericApiController<ItProject, int, ItProjectDTO>
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;

        public ItProjectController(IGenericRepository<ItProject> repository, IGenericRepository<OrganizationUnit> orgUnitRepository) 
            : base(repository)
        {
            _orgUnitRepository = orgUnitRepository;
        }

        public HttpResponseMessage GetOrganizationUnitsUsingThisProject(int id, [FromUri] int organizationUnit)
        {
            try
            {
                var usage = Repository.GetByKey(id);

                if (usage == null) return NotFound();

                return Ok(Map<IEnumerable<OrganizationUnit>, IEnumerable<OrgUnitDTO>>(usage.UsedByOrgUnits));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage PostOrganizationUnitsUsingThisProject(int id, [FromUri] int organizationUnit)
        {
            try
            {
                var usage = Repository.GetByKey(id);
                var orgUnit = _orgUnitRepository.GetByKey(organizationUnit);

                if (usage == null || orgUnit == null) return NotFound();

                usage.UsedByOrgUnits.Add(orgUnit);
                Repository.Save();

                return Created(Map<OrganizationUnit, OrgUnitDTO>(orgUnit));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage DeleteOrganizationUnitsUsingThisProject(int id, [FromUri] int organizationUnit)
        {
            try
            {
                var usage = Repository.GetByKey(id);
                var orgUnit = _orgUnitRepository.GetByKey(organizationUnit);

                if (usage == null || orgUnit == null) return NotFound();

                usage.UsedByOrgUnits.Remove(orgUnit);
                Repository.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}