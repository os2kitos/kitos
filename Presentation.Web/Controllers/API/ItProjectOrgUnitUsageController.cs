using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [DenyRightsHoldersAccess]
    public class ItProjectOrgUnitUsageController : BaseApiController
    {
        private readonly IGenericRepository<ItProjectOrgUnitUsage> _responsibleOrgUnitRepository;
        private readonly IGenericRepository<ItProject> _projectRepository;

        public ItProjectOrgUnitUsageController(IGenericRepository<ItProjectOrgUnitUsage> responsibleOrgUnitRepository, IGenericRepository<ItProject> projectRepository)
        {
            _responsibleOrgUnitRepository = responsibleOrgUnitRepository;
            _projectRepository = projectRepository;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<SimpleOrgUnitDTO>>))]
        public HttpResponseMessage GetOrgUnitsByProject(int id)
        {
            try
            {
                var itProject = _projectRepository.GetByKey(id);
                
                if (itProject == null)
                    return NotFound();
                
                if (!AllowRead(itProject))
                    return Forbidden();

                var items = _responsibleOrgUnitRepository.Get(x => x.ItProjectId == id);
                var orgUnits = items.Select(x => x.OrganizationUnit);

                var dtos = Mapper.Map<IEnumerable<SimpleOrgUnitDTO>>(orgUnits);

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<SimpleOrgUnitDTO>))]
        public HttpResponseMessage GetResponsibleByProject(int id, bool? responsible)
        {
            try
            {
                var project = _projectRepository.GetByKey(id);

                if (project?.ResponsibleUsage == null) return Ok(); // TODO should be NotFound but ui router resolve redirects to mainpage on 404

                var organizationUnit = project.ResponsibleUsage.OrganizationUnit;

                if (!AllowRead(organizationUnit))
                {
                    return Forbidden();
                }

                var dtos = Mapper.Map<SimpleOrgUnitDTO>(organizationUnit);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage PostSetResponsibleOrgUnit(int projectId, int orgUnitId, bool? responsible)
        {
            try
            {
                var entity = _responsibleOrgUnitRepository.GetByKey(new object[] { projectId, orgUnitId });
                var project = _projectRepository.GetByKey(projectId);

                if (project == null)
                {
                    return NotFound();
                }

                if (!AllowModify(project))
                {
                    return Forbidden();
                }

                project.ResponsibleUsage = entity;

                _responsibleOrgUnitRepository.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage DeleteResponsibleOrgUnit(int projectId, bool? responsible)
        {
            try
            {
                var project = _projectRepository.GetByKey(projectId);

                if (project == null)
                {
                    return NotFound();
                }

                if (!AllowModify(project))
                {
                    return Forbidden();
                }

                // WARNING: force loading so setting it to null will be tracked
                var forceLoad = project.ResponsibleUsage;
                project.ResponsibleUsage = null;

                _projectRepository.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }
    }
}
