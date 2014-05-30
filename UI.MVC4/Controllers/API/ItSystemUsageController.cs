using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItSystemUsageController : GenericApiController<ItSystemUsage, ItSystemUsageDTO, ItSystemUsageDTO> 
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;
        private readonly IGenericRepository<TaskRef> _taskRepository;
        private readonly IItSystemUsageService _itSystemUsageService;

        public ItSystemUsageController(IGenericRepository<ItSystemUsage> repository, IGenericRepository<OrganizationUnit> orgUnitRepository, IGenericRepository<TaskRef> taskRepository, IItSystemUsageService itSystemUsageService) 
            : base(repository)
        {
            _orgUnitRepository = orgUnitRepository;
            _taskRepository = taskRepository;
            _itSystemUsageService = itSystemUsageService;
        }

        public HttpResponseMessage GetSearchByOrganization(int organizationId, string q)
        {
            try
            {
                var usages = Repository.Get(u => u.OrganizationId == organizationId && u.ItSystem.Name.StartsWith(q));

                return Ok(Map(usages));
            }
            catch (Exception e)
            {
                return Error(e);
            } 
        }

        public HttpResponseMessage GetByOrganization(int organizationId)
        {
            try
            {
                var usages = Repository.Get(u => u.OrganizationId == organizationId);
                
                return Ok(Map(usages));

            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetByItSystemAndOrganization(int itSystemId, int organizationId)
        {
            try
            {
                var usage = Repository.Get(u => u.ItSystemId == itSystemId && u.OrganizationId == organizationId).FirstOrDefault();

                return usage == null ? NotFound() : Ok(Map(usage));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public override HttpResponseMessage Post(ItSystemUsageDTO dto)
        {
            try
            {
                if (Repository.Get(usage => usage.ItSystemId == dto.ItSystemId 
                    && usage.OrganizationId == dto.OrganizationId).Any())
                    return Conflict("Usage already exist");

                var sysUsage = _itSystemUsageService.Add(dto.ItSystemId, dto.OrganizationId, KitosUser);

                return Created(Map(sysUsage), new Uri(Request.RequestUri + "?itSystemId=" + dto.ItSystemId + "&organizationId" + dto.OrganizationId));

            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage Delete(int itSystemId, int organizationId)
        {
            try
            {
                var usage = Repository.Get(u => u.ItSystemId == itSystemId && u.OrganizationId == organizationId).FirstOrDefault();

                if (usage == null) return NotFound();

                Repository.DeleteByKey(usage.Id);
                Repository.Save();

                return Ok();

            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetOrganizationUnitsUsingThisSystem(int id, [FromUri] int organizationUnit)
        {
            try
            {
                var usage = Repository.GetByKey(id);

                if (usage == null) return NotFound();

                return Ok(Map<IEnumerable<OrganizationUnit>, IEnumerable<OrgUnitDTO>>(usage.UsedBy));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage PostOrganizationUnitsUsingThisSystem(int id, [FromUri] int organizationUnit)
        {
            try
            {
                var usage = Repository.GetByKey(id);
                var orgUnit = _orgUnitRepository.GetByKey(organizationUnit);

                if (usage == null || orgUnit == null) return NotFound();

                usage.UsedBy.Add(orgUnit);
                Repository.Save();

                return Created(Map<OrganizationUnit, OrgUnitDTO>(orgUnit));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage DeleteOrganizationUnitsUsingThisSystem(int id, [FromUri] int organizationUnit)
        {
            try
            {
                var usage = Repository.GetByKey(id);
                var orgUnit = _orgUnitRepository.GetByKey(organizationUnit);

                if (usage == null || orgUnit == null) return NotFound();

                usage.UsedBy.Remove(orgUnit);
                Repository.Save();
                
                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetTasksUsedByThisSystem(int id, [FromUri] int taskId)
        {
            try
            {
                var usage = Repository.GetByKey(id);

                if (usage == null) return NotFound();

                return Ok(Map<IEnumerable<TaskRef>, IEnumerable<TaskRefDTO>>(usage.TaskRefs));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage PostTasksUsedByThisSystem(int id, [FromUri] int taskId)
        {
            try
            {
                var usage = Repository.GetByKey(id);
                var task = _taskRepository.GetByKey(taskId);

                if (usage == null || task == null) return NotFound();

                usage.TaskRefs.Add(task);
                Repository.Save();

                return Created(Map<TaskRef, TaskRefDTO>(task));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage DeleteTasksUsedByThisSystem(int id, [FromUri] int taskId)
        {
            try
            {
                var usage = Repository.GetByKey(id);
                var task = _taskRepository.GetByKey(taskId);

                if (usage == null || task == null) return NotFound();

                usage.TaskRefs.Remove(task);
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
