using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public abstract class GenericApiController<TModel, TKeyType, TDto> : BaseApiController // TODO perhaps it's possible to infer the TKeyType from TModel somehow
        where TModel : class, IEntity<TKeyType>
    {
        protected readonly IGenericRepository<TModel> Repository;

        protected GenericApiController(IGenericRepository<TModel> repository)
        {
            Repository = repository;
        }

        protected virtual TDest Map<TSource, TDest>(TSource item)
        {
            return AutoMapper.Mapper.Map<TDest>(item);
        }

        protected virtual IEnumerable<TModel> GetAllQuery()
        {
            return Repository.Get();
        }

        public HttpResponseMessage GetAll()
        {
            var items = GetAllQuery().ToList();

            if (!items.Any())
                return NoContent();

            return Ok(Map<IEnumerable<TModel>, IEnumerable<TDto>>(items));
        }

        // GET api/T
        public HttpResponseMessage GetSingle(TKeyType id)
        {
            var item = Repository.GetByKey(id);

            if (item == null)
                return NoContent();

            return Ok(Map<TModel, TDto>(item));
        }

        protected virtual TModel PostQuery(TModel item)
        {
            Repository.Insert(item);
            Repository.Save();

            return item;
        }

        // POST api/T
        [Authorize(Roles = "GlobalAdmin")]
        public HttpResponseMessage Post(TDto dto)
        {
            var item = Map<TDto, TModel>(dto);
            try
            {
                PostQuery(item);

                //var msg = new HttpResponseMessage(HttpStatusCode.Created);
                return Created(item, new Uri(Request.RequestUri + "/" + item.Id));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        protected virtual TModel PutQuery(TModel item)
        {
            Repository.Update(item);
            Repository.Save();

            return item;
        }

        // PUT api/T
        [Authorize(Roles = "GlobalAdmin")]
        public HttpResponseMessage Put(TKeyType id, TDto dto)
        {
            var item = Map<TDto, TModel>(dto);
            item.Id = id;
            try
            {
                PutQuery(item);

                return Ok(); //TODO correct?
            }
            catch (Exception)
            {
                return NoContent(); // TODO catch correct expection
            }
        }

        protected virtual void DeleteQuery(TKeyType id)
        {
            Repository.DeleteByKey(id);
            Repository.Save();
        }

        // DELETE api/T
        [Authorize(Roles = "GlobalAdmin")]
        public HttpResponseMessage Delete(TKeyType id)
        {
            try
            {
                DeleteQuery(id);

                return Ok();
            }
            catch (Exception)
            {
                return NoContent(); // TODO catch correct expection
            }
        }

        protected virtual TModel PatchQuery(TModel item)
        {
            Repository.Patch(item);
            Repository.Save();

            return item;
        }

        // PATCH api/T
        [Authorize(Roles = "GlobalAdmin")]
        public HttpResponseMessage Patch(TKeyType id, TDto dto)
        {
            var item = Map<TDto, TModel>(dto);
            item.Id = id;

            try
            {
                PatchQuery(item);

                //pretty sure we'll get a merge conflict here???
                return Ok(); // TODO correct?
            }
            catch (Exception)
            {
                return NoContent(); // TODO catch correct expection
            }
        }

        protected override void Dispose(bool disposing)
        {
            Repository.Dispose();
            base.Dispose(disposing);
        }
    }
}
