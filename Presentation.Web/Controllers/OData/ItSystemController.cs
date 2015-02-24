using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Infrastructure.DataAccess;

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
        public SingleResult<ItSystem> Get([FromODataUri] int key)
        {
            IQueryable<ItSystem> result = db.ItSystems.Where(x => x.Id == key);
            return SingleResult.Create(result);
        }

        public async Task<IHttpActionResult> Post(ItSystem itSystem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            db.ItSystems.Add(itSystem);
            await db.SaveChangesAsync();
            return Created(itSystem);
        }

        public async Task<IHttpActionResult> Patch([FromODataUri] int key, Delta<ItSystem> itSystem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var entity = await db.ItSystems.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }

            itSystem.Patch(entity);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SystemExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Updated(entity);
        }

        public async Task<IHttpActionResult> Put([FromODataUri] int key, ItSystem update)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != update.Id)
            {
                return BadRequest();
            }
            db.Entry(update).State = EntityState.Modified;
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SystemExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Updated(update);
        }

        public async Task<IHttpActionResult> Delete([FromODataUri] int key)
        {
            var itSystem = await db.ItSystems.FindAsync(key);
            if (itSystem == null)
            {
                return NotFound();
            }
            db.ItSystems.Remove(itSystem);
            await db.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        

    }
}