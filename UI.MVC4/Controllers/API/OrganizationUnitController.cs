using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class OrganizationUnitController : GenericApiController<OrganizationUnit, int, OrgUnitDTO>
    {
        private readonly IGenericRepository<TaskRef> _taskRepository;
        private readonly IOrgUnitService _orgUnitService;

        public OrganizationUnitController(IGenericRepository<OrganizationUnit> repository, IGenericRepository<TaskRef> taskRepository, IOrgUnitService orgUnitService) 
            : base(repository)
        {
            _taskRepository = taskRepository;
            _orgUnitService = orgUnitService;
        }

        public HttpResponseMessage GetByUser(int userId)
        {
            try
            {
                var user = KitosUser;

                if(user.Id != userId) throw new SecurityException();

                var orgUnits = _orgUnitService.GetByUser(user);

                return Ok(Map<ICollection<OrganizationUnit>, ICollection<OrgUnitDTO>>(orgUnits));

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
                var orgUnit = Repository.Get(o => o.Organization_Id == organization && o.Parent == null).FirstOrDefault();

                if (orgUnit == null) return NotFound();

                var item = Map<OrganizationUnit, OrgUnitDTO>(orgUnit);

                return Ok(item);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetTaskRefs(int id, [FromUri] bool? taskRefs)
        {
            var refs = Repository.Get(x => x.Id == id).SelectMany(x => x.TaskRefs);
            return Ok(Map<IEnumerable<TaskRef>, IEnumerable<TaskRefDTO>>(refs));
        }

        public HttpResponseMessage PostTaskRef(int id, [FromUri] int taskRef)
        {
            var taskRefEntity = _taskRepository.GetByKey(taskRef);
            var orgUnit = Repository.GetByKey(id);
            orgUnit.TaskRefs.Add(taskRefEntity);
            Repository.Update(orgUnit);
            Repository.Save();
            return NoContent(); // TODO figure out what to return when refs are posted
        }

        public HttpResponseMessage DeleteTaskRef(int id, [FromUri] int taskRef)
        {
            var taskRefEntity = _taskRepository.GetByKey(taskRef);
            var orgUnit = Repository.GetByKey(id);
            orgUnit.TaskRefs.Remove(taskRefEntity);
            Repository.Update(orgUnit);
            Repository.Save();
            return NoContent(); // TODO figure out what to return when refs are posted
        }
    }
}
