using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Query;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class BaseController<T> : ODataController where T : class
    {
        protected ODataValidationSettings ValidationSettings = new ODataValidationSettings();
        protected IGenericRepository<T> Repo;

        public BaseController(IGenericRepository<T> repository)
        {
            ValidationSettings.AllowedQueryOptions = AllowedQueryOptions.All;
            Repo = repository;
        }

        [EnableQuery]
        public virtual IQueryable<T> Get()
        {
            return Repo.AsQueryable();
        }

        [EnableQuery(MaxExpansionDepth = 4)]
        public virtual IQueryable<T> Get([FromODataUri] int key)
        {
            var result = new List<T> {Repo.GetByKey(key)};
            return result.AsQueryable();
        }

        public virtual IQueryable<T> GetQueryable(ODataQueryOptions<T> queryOptions)
        {
            return Repo.AsQueryable();
        }

        public virtual IQueryable<T> GetQueryable(int key, ODataQueryOptions<T> queryOptions)
        {
            var result = new List<T>();

            var entity = Repo.GetByKey(key);
            result.Add(entity);

            return result.AsQueryable();
        }

        protected IHttpActionResult Put(int key, Delta<T> delta)
        {
            return StatusCode(HttpStatusCode.MethodNotAllowed);
        }

        protected IHttpActionResult POst(T entity)
        {
            Validate(entity);

            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                entity = Repo.Insert(entity);
                Repo.Save();
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }

            return Created(entity);
        }

        protected IHttpActionResult Patch(int key, Delta<T> delta)
        {
            Validate(delta.GetEntity());

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entity = Repo.GetByKey(key);
            if(entity == null) return BadRequest("Unable to find entity with id " + key);

            try
            {
                delta.Patch(entity);
                Repo.Save();
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }

            return Updated(entity);
        }

        protected IHttpActionResult Delete(int key)
        {
            try
            {
                Repo.DeleteByKey(key);
                Repo.Save();
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }

            return Ok();
        }
    }
}
