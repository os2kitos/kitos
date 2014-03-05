using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public abstract class GenericLocaleApiController<TModel, TOriginal> : ApiController
        where TModel : class, ILocaleEntity<TOriginal>
    {
        protected readonly IGenericRepository<TModel> Repository;

        protected GenericLocaleApiController(IGenericRepository<TModel> repository)
        {
            Repository = repository;
        }

        protected virtual TDest Map<TSource, TDest>(TSource item)
        {
            return AutoMapper.Mapper.Map<TDest>(item);
        }

        protected virtual IEnumerable<TModel> GetAllQuery()
        {
            return Repository.Get(l => l.Municipality_Id == 1);
        }

        protected virtual TModel PostQuery(TModel item)
        {
            Repository.Insert(item);
            Repository.Save();

            return item;
        }

        protected virtual TModel PutQuery(TModel item)
        {
            Repository.Update(item);
            Repository.Save();

            return item;
        }

        public HttpResponseMessage GetAll()
        {
            var items = GetAllQuery().ToList();

            if (!items.Any())
                return Request.CreateResponse(HttpStatusCode.NoContent);

            return Request.CreateResponse(HttpStatusCode.OK, Map<IEnumerable<TModel>, IEnumerable<LocaleDTO>>(items));
        }
        
        public HttpResponseMessage GetAllFromMunicipality([FromUri] int mId)
        {
            var items = Repository.Get(l => l.Municipality_Id == mId).ToList();

            if (!items.Any())
                return Request.CreateResponse(HttpStatusCode.NoContent);

            return Request.CreateResponse(HttpStatusCode.OK, Map<IEnumerable<TModel>, IEnumerable<LocaleDTO>>(items));
        }

        // GET api/T
        public HttpResponseMessage GetSingle([FromUri] int mId, [FromUri] int oId)
        {
            var item = Repository.GetByKey(mId, oId);

            if (item == null)
                return Request.CreateResponse(HttpStatusCode.NoContent);

            return Request.CreateResponse(HttpStatusCode.OK, Map<TModel, LocaleDTO>(item));
        }

        // POST api/T
        [Authorize(Roles = "GlobalAdmin")]
        public HttpResponseMessage Post(LocaleDTO dto)
        {
            var item = Map<LocaleDTO, TModel>(dto);
            item.Municipality_Id = 1;
            try
            {
                PostQuery(item);

                //var msg = new HttpResponseMessage(HttpStatusCode.Created);
                var msg = Request.CreateResponse(HttpStatusCode.Created, item);
                msg.Headers.Location = new Uri(Request.RequestUri + "?mId=" + item.Municipality_Id + "&oId=" + item.Original_Id);
                return msg;
            }
            catch (Exception)
            {
                throw new HttpResponseException(HttpStatusCode.Conflict); // TODO catch correct expection
            }
        }

        // PUT api/T
        [Authorize(Roles = "GlobalAdmin")]
        public HttpResponseMessage Put(LocaleDTO dto)
        {
            var item = Map<LocaleDTO, TModel>(dto);
            item.Municipality_Id = 1;
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
    }
}
