using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Security;
using Core.DomainModel.Text;
using Core.DomainServices;
using Infrastructure.DataAccess;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class KitosIntroApiController : ApiController
    {
        private readonly IGenericRepository<KitosIntro> _repository;

        public KitosIntroApiController(IGenericRepository<KitosIntro> repository)
        {
            _repository = repository;
        }

        // GET api/kitosintroapi
        public IEnumerable<KitosIntro> Get()
        {
            return _repository.Get();
        }

        // GET api/kitosintroapi/5
        public KitosIntro Get(int id)
        {
            return _repository.GetById(id);
        }

        // POST api/kitosintroapi
        [Authorize(Roles = "Admin")]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/kitosintroapi/5
        [Authorize(Roles = "Admin")]
        public HttpResponseMessage Put(int id, [FromBody] XeditableReturnModel returnModel)
        {
            try
            {
                _repository.Update(new KitosIntro()
                {
                    Id = id,
                    Text = returnModel.Value
                });
                _repository.Save();

                return new HttpResponseMessage(HttpStatusCode.OK); // TODO correct?
            }
            catch (Exception)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }

        // DELETE api/kitosintroapi/5
        public void Delete(int id)
        {
        }
    }
}
