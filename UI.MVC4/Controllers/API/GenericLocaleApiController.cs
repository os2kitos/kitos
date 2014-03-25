using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public abstract class GenericLocaleApiController<TModel, TOriginal> : BaseApiController
        where TModel : class, ILocaleEntity<TOriginal>
    {
        protected readonly IGenericRepository<TModel> Repository;
        private readonly IUserRepository _userRepository;

        protected GenericLocaleApiController(IGenericRepository<TModel> repository, IUserRepository userRepository)
        {
            Repository = repository;
            _userRepository = userRepository;
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
                return NoContent();

            return Ok(Map<IEnumerable<TModel>, IEnumerable<LocaleDTO>>(items));
        }
        
        public HttpResponseMessage Get(int id)
        {
            var items = Repository.Get(l => l.Municipality_Id == id).ToList();

            if (!items.Any())
                return NoContent();

            return Ok(Map<IEnumerable<TModel>, IEnumerable<LocaleDTO>>(items));
        }

        // GET api/T
        public HttpResponseMessage GetSingle([FromUri] int orgId, [FromUri] int oId)
        {
            var item = Repository.GetByKey(mId, oId);

            if (item == null)
                return NoContent();

            return Ok(Map<TModel, LocaleDTO>(item));
        }

        // POST api/T
        public HttpResponseMessage Post(LocaleInputDTO dto)
        {
            var item = Map<LocaleInputDTO, TModel>(dto);
            try
            {
                TestMunicipalityMembership(dto.Municipality_Id);

                PostQuery(item);

                //var msg = new HttpResponseMessage(HttpStatusCode.Created);
                return Created(item,
                               new Uri(Request.RequestUri + "?orgId=" + item.Municipality_Id + "&oId=" + item.Original_Id));
            }
            catch (Exception e)
            {
                return Error(e); // TODO catch correct expection
            }
        }

        // PUT api/T
        public HttpResponseMessage Put(LocaleInputDTO dto)
        {
            var item = Map<LocaleInputDTO, TModel>(dto);
            try
            {
                TestMunicipalityMembership(dto.Municipality_Id);

                PutQuery(item);

                return Ok(); // TODO correct?
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        // PUT api/T
        public HttpResponseMessage Delete([FromUri] int orgId, [FromUri] int oId)
        {
            try
            {
                TestMunicipalityMembership(mId);

                Repository.DeleteByKey(mId, oId);
                Repository.Save();

                return Ok(); // TODO correct?
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        private void TestMunicipalityMembership(int mId)
        {
            //throw new NotImplementedException();
        }
    }
}
