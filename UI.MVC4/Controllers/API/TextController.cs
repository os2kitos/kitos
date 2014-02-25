using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers
{
    public class TextController : GenericApiController<Text, string>
    {
        public TextController(IGenericRepository<Text> repository)
            : base(repository)
        {
        }

        // PUT api/Text
        public HttpResponseMessage Put(string id, [FromBody] XeditableReturnModel model)
        {
            var item = new Text() {Id = id, Description = model.Value};
            return base.Put(id, item);
        }
    }
}
