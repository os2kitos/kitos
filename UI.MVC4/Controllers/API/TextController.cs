using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;

namespace UI.MVC4.Controllers
{
    public class TextController : GenericApiController<Text, string>
    {
        public TextController(IGenericRepository<Text> repository)
            : base(repository)
        {
        }

        // PUT api/T
        [HttpPut]
        public HttpResponseMessage Put([FromUri] string pk, [FromUri] string value, [FromUri] string name)
        {
            var item = new Text() { Id = pk, Description = value};
            
            try
            {
                Repository.Update(item);
                Repository.Save();

                return new HttpResponseMessage(HttpStatusCode.OK); // TODO correct?
            }
            catch (Exception)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }
    }
}
