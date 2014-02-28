using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;

namespace UI.MVC4.Controllers
{
    public abstract class GenericApiController<TModel, TKeyType, TDto> : ApiController // TODO perhaps it's possible to infer the TKeyType from TModel somehow
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
                return Request.CreateResponse(HttpStatusCode.NoContent);

            return Request.CreateResponse(HttpStatusCode.OK, Map<IEnumerable<TModel>, IEnumerable<TDto>>(items));
        }

        // GET api/T
        public HttpResponseMessage GetSingle(TKeyType id)
        {
            var item = Repository.GetById(id);

            if (item == null)
                return Request.CreateResponse(HttpStatusCode.NoContent);

            return Request.CreateResponse(HttpStatusCode.OK, Map<TModel, TDto>(item));
        }

        protected TModel PostQuery(TModel item)
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
                var msg = Request.CreateResponse(HttpStatusCode.Created, item);
                msg.Headers.Location = new Uri(Request.RequestUri + "/" + item.Id);
                return msg;
            }
            catch (Exception)
            {
                throw new HttpResponseException(HttpStatusCode.Conflict); // TODO catch correct expection
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

                return new HttpResponseMessage(HttpStatusCode.OK); // TODO correct?
            }
            catch (Exception)
            {
                throw new HttpResponseException(HttpStatusCode.NoContent); // TODO catch correct expection
            }
        }

        protected virtual void DeleteQuery(TKeyType id)
        {
            Repository.DeleteById(id);
            Repository.Save();
        }

        // DELETE api/T
        [Authorize(Roles = "GlobalAdmin")]
        public HttpResponseMessage Delete(TKeyType id)
        {
            try
            {
                DeleteQuery(id);

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception)
            {
                throw new HttpResponseException(HttpStatusCode.NoContent);
            }
        }

        protected override void Dispose(bool disposing)
        {
            Repository.Dispose();
            base.Dispose(disposing);
        }
    }
}
