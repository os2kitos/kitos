using System;
using System.Collections.Generic;
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

        public virtual HttpResponseMessage Get()
        {
            var items = Repository.Get();

            if (items == null)
                return Request.CreateResponse(HttpStatusCode.NoContent);

            return Request.CreateResponse(HttpStatusCode.OK, AutoMapper.Mapper.Map<IEnumerable<TDto>>(items));
        }

        // GET api/T
        public virtual HttpResponseMessage Get(TKeyType id)
        {
            var item = Repository.GetById(id);

            if (item == null)
                return Request.CreateResponse(HttpStatusCode.NoContent);

            return Request.CreateResponse(HttpStatusCode.OK, AutoMapper.Mapper.Map<TDto>(item));
        }

        // POST api/T
        [Authorize(Roles = "GlobalAdmin")]
        public virtual HttpResponseMessage Post(TDto dto)
        {
            var item = AutoMapper.Mapper.Map<TModel>(dto);
            try
            {
                Repository.Insert(item);
                Repository.Save();

                //var msg = new HttpResponseMessage(HttpStatusCode.Created);
                var msg = Request.CreateResponse(HttpStatusCode.Created, item);
                msg.Headers.Location = new Uri(Request.RequestUri + "/" + item.Id.ToString());
                return msg;
            }
            catch (Exception)
            {
                throw new HttpResponseException(HttpStatusCode.Conflict); // TODO catch correct expection
            }
        }

        // PUT api/T
        [Authorize(Roles = "GlobalAdmin")]
        public virtual HttpResponseMessage Put(TKeyType id, TDto dto)
        {
            var item = AutoMapper.Mapper.Map<TModel>(dto);
            item.Id = id;
            try
            {
                Repository.Update(item);
                Repository.Save();

                return new HttpResponseMessage(HttpStatusCode.OK); // TODO correct?
            }
            catch (Exception)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound); // TODO catch correct expection
            }
        }

        // DELETE api/T
        [Authorize(Roles = "GlobalAdmin")]
        public virtual HttpResponseMessage Delete(TKeyType id)
        {
            try
            {
                Repository.DeleteById(id);
                Repository.Save();

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }

        protected override void Dispose(bool disposing)
        {
            Repository.Dispose();
            base.Dispose(disposing);
        }
    }
}
