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
    public class MunicipalityController : GenericApiController<Municipality, int, MunicipalityApiModel>
    {
        public MunicipalityController(IGenericRepository<Municipality> repository) : base(repository)
        {
        }
    }

    /* I don't remember why I didn't just use generic api controller?
    public class MunicipalityController : ApiController
    {
        private readonly IGenericRepository<Municipality> _repository;

        public MunicipalityController(IGenericRepository<Municipality> repository)
        {
            _repository = repository;
        }

        // GET api/T
        [Authorize]
        public MunicipalityApiModel Get(int id)
        {
            return AutoMapper.Mapper.Map<Municipality, MunicipalityApiModel>(_repository.GetByKey(id));
        }

        [Authorize]
        public IEnumerable<MunicipalityApiModel> Get()
        {
            var municipalities = _repository.Get();
            return AutoMapper.Mapper.Map<IEnumerable<Municipality>, List<MunicipalityApiModel>>(municipalities);
        }


        [Authorize]
        public IEnumerable<MunicipalityApiModel> Get(string term)
        {
            var municipalities = _repository.Get(m => m.Name.StartsWith(term));
            return AutoMapper.Mapper.Map<IEnumerable<Municipality>, List<MunicipalityApiModel>>(municipalities);
        }

        // POST api/T
        [Authorize(Roles = "GlobalAdmin")]
        public HttpResponseMessage Post(MunicipalityApiModel item)
        {
            try
            {
                var municipality = AutoMapper.Mapper.Map<MunicipalityApiModel, Municipality>(item);
                _repository.Insert(municipality);
                _repository.Save();
                
                var msg = Request.CreateResponse(HttpStatusCode.Created, AutoMapper.Mapper.Map<Municipality,MunicipalityApiModel>(municipality));
                msg.Headers.Location = new Uri(Request.RequestUri + item.Id.ToString());
                return msg;
            }
            catch (Exception)
            {
                throw new HttpResponseException(HttpStatusCode.Conflict);
            }
        }

        // PUT api/T
        [Authorize(Roles = "GlobalAdmin")]
        public HttpResponseMessage Put(int id, MunicipalityApiModel item)
        {
            item.Id = id;
            try
            {
                var municipality = AutoMapper.Mapper.Map<MunicipalityApiModel, Municipality>(item);
                _repository.Update(municipality);
                _repository.Save();
                
                var msg = Request.CreateResponse(HttpStatusCode.Created, AutoMapper.Mapper.Map<Municipality,UserDTO>(municipality));
                return new HttpResponseMessage(HttpStatusCode.OK); // TODO correct?
            }
            catch (Exception)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }

        // DELETE api/T
        [Authorize(Roles = "GlobalAdmin")]
        public virtual HttpResponseMessage Delete(int id)
        {
            try
            {
                _repository.DeleteByKey(id);
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
     * */
}
