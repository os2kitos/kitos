using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.Text;
using Core.DomainServices;
using Infrastructure.DataAccess;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class KitosIntroApiController : ApiController
    {
        private readonly IGenericRepository<KitosIntro> _repository;

        public KitosIntroApiController()
        {
            //yeah yeah, I know, we should use DI
            _repository = new FakeKitosIntroRepository();
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
        public void Post([FromBody]string value)
        {
        }

        // PUT api/kitosintroapi/5
        public void Put(int id, [FromBody] XeditableReturnModel returnModel)
        {
            _repository.Update(new KitosIntro()
                {
                    Id = id,
                    Text = returnModel.Value
                });
        }

        // DELETE api/kitosintroapi/5
        public void Delete(int id)
        {
        }
    }
}
