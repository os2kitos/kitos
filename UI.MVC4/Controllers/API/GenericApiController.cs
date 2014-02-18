using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainServices;

namespace UI.MVC4.Controllers
{
    public abstract class GenericApiController<T> : ApiController 
        where T : class
    {
        private readonly IGenericRepository<T> _repository;

        protected GenericApiController(IGenericRepository<T> repository)
        {
            _repository = repository;
        }

        // GET api/T
        public T Get(int id)
        {
            return _repository.GetById(id);
        }

        // POST api/T
        public HttpResponseMessage Post(T item)
        {
            try
            {
                _repository.Insert(item);
                _repository.Save();

                var msg = new HttpResponseMessage(HttpStatusCode.Created);
                //msg.Headers.Location = new Uri(Request.RequestUri + newItem.ID.ToString()); TODO
                return msg;
            }
            catch (Exception)
            {
                throw new HttpResponseException(HttpStatusCode.Conflict);
            }
        }

        // PUT api/T
        public HttpResponseMessage Put(int id, T item)
        {
            // item.Id = id; TODO
            try
            {
                _repository.Update(item);
                _repository.Save();

                return new HttpResponseMessage(HttpStatusCode.OK); // TODO correct?
            }
            catch (Exception)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }

        // DELETE api/T
        public HttpResponseMessage Delete(int id)
        {
            try
            {
                _repository.DeleteById(id);
                _repository.Save();

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }

        protected override void Dispose(bool disposing)
        {
            _repository.Dispose();
            base.Dispose(disposing);
        }
    }
}
