using System;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Query;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class BaseController<T> : ODataController where T : class
    {
        protected ODataValidationSettings ValidationSettings = new ODataValidationSettings();
        protected IGenericRepository<T> Repository;

        public BaseController(IGenericRepository<T> repository)
        {
            ValidationSettings.AllowedQueryOptions = AllowedQueryOptions.All;
            Repository = repository;
        }

        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(Repository.AsQueryable());
        }

        [EnableQuery(MaxExpansionDepth = 4)]
        public IHttpActionResult Get(int key)
        {
            var entity = Repository.GetByKey(key);
            return Ok(entity);
        }

        //public IQueryable<T> GetQueryable(ODataQueryOptions<T> queryOptions)
        //{
        //    return Repository.AsQueryable();
        //}

        //public IQueryable<T> GetQueryable(int key, ODataQueryOptions<T> queryOptions)
        //{
        //    var result = new List<T>();

        //    var entity = Repository.GetByKey(key);
        //    result.Add(entity);

        //    return result.AsQueryable();
        //}

        protected IHttpActionResult Put(int key, Delta<T> delta)
        {
            return StatusCode(HttpStatusCode.MethodNotAllowed);
        }

        protected IHttpActionResult Post(T entity)
        {
            Validate(entity);

            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                entity = Repository.Insert(entity);
                Repository.Save();
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

            var entity = Repository.GetByKey(key);
            if(entity == null) 
                return NotFound();

            try
            {
                delta.Patch(entity);
                Repository.Save();
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }

            return Updated(entity);
        }

        protected IHttpActionResult Delete(int key)
        {
            var entity = Repository.GetByKey(key);
            if (entity == null)
                return NotFound();
            
            try
            {
                Repository.DeleteByKey(key);
                Repository.Save();
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }

            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
