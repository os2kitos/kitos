using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.OData;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Infrastructure.DataAccess;
using Ninject.Infrastructure.Language;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.OData
{
    public class ItSystemsController : ODataController
    {
        private readonly IGenericRepository<TaskRef> _taskRepository;
        private readonly IItSystemService _systemService;
        private readonly IGenericRepository<ItSystem> _itSystemRepository;
        public ItSystemsController(IGenericRepository<ItSystem> itSystemRepository, IGenericRepository<ItSystem> repository, IGenericRepository<TaskRef> taskRepository, IItSystemService systemService)
        {
            _taskRepository = taskRepository;
            _systemService = systemService;
            _itSystemRepository = itSystemRepository;
        }
        
        KitosContext db = new KitosContext();

        private bool SystemExists(int key)
        {
            return db.ItSystems.Any(s => s.Id == key);
        }

        [EnableQuery]
        public IQueryable<ItSystem> Get()
        {
            return _itSystemRepository.AsQueryable();
        }

        [EnableQuery]
        public ItSystem Get([FromODataUri] int key)
        {
            return _itSystemRepository.GetByKey(key);
        }

        public IHttpActionResult Post(ItSystem itSystem)
        {
            try
            {
                var created = _itSystemRepository.Insert(itSystem);
                _itSystemRepository.Save();

                return Created(created);

            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        public IHttpActionResult Patch([FromODataUri] int key, Delta<ItSystem> delta)
        {
            Validate(delta.GetEntity());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var entity = _itSystemRepository.GetByKey(key);
            if (entity == null)
            {
                return NotFound();
            }

            try
            {
                delta.Patch(entity);
                _itSystemRepository.Save();

                return Updated(entity);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        public IHttpActionResult Put([FromODataUri] int key, ItSystem update)
        {
            return StatusCode(HttpStatusCode.MethodNotAllowed);
        }

        public IHttpActionResult Delete([FromODataUri] int key)
        {
            var entity = _itSystemRepository.GetByKey(key);
            if (entity == null)
            {
                return NotFound();
            }
            try
            {
                //Get children
                var systems = _systemService.GetHierarchy(key);

                //Set each child's parent to null
                foreach (var system in systems)
                {
                    if(system == entity) continue;

                    system.Parent = null;
                }

                _itSystemRepository.Save();

                //Delete ItSystem
                _itSystemRepository.DeleteByKey(key);
                _itSystemRepository.Save();

                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }
    }
}