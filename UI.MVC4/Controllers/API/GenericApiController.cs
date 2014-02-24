using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;

namespace UI.MVC4.Controllers
{
    public abstract class GenericApiController<TModel, TKeyType> : ApiController // TODO perhaps it's possible to infer the TKeyType from TModel somehow
        where TModel : class, IEntity<TKeyType>
    {
        private readonly IGenericRepository<TModel> _repository;

        protected GenericApiController(IGenericRepository<TModel> repository)
        {
            _repository = repository;
        }

        // GET api/T
        public TModel Get(TKeyType id)
        {
            return _repository.GetById(id);
        }

        // POST api/T
        public HttpResponseMessage Post(TModel item)
        {
            try
            {
                _repository.Insert(item);
                _repository.Save();

                var msg = new HttpResponseMessage(HttpStatusCode.Created);
                msg.Headers.Location = new Uri(Request.RequestUri + item.Id.ToString());
                return msg;
            }
            catch (Exception)
            {
                throw new HttpResponseException(HttpStatusCode.Conflict);
            }
        }

        // PUT api/T
        public HttpResponseMessage Put(TKeyType id, TModel item)
        {
            item.Id = id;
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
        public HttpResponseMessage Delete(TKeyType id)
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
