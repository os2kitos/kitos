using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class MunicipalityController : GenericApiController<Municipality, int>
    {
        public MunicipalityController(IGenericRepository<Municipality> repository)
            : base(repository)
        {
        }

        /*

        protected readonly IGenericRepository<Municipality> Repository;

        public MunicipalityController(IGenericRepository<Municipality> repository)
        {
            Repository = repository;
        }

        // GET api/T
        public virtual Municipality Get(int id)
        {
            return Repository.GetById(id);
        }

        // POST api/T
        [Authorize(Roles = "Admin")]
        public virtual HttpResponseMessage Post(Municipality item)
        {
            try
            {
                Repository.Insert(item);
                Repository.Save();

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
        [Authorize(Roles = "Admin")]
        public virtual HttpResponseMessage Put(int id, Municipality item)
        {
            item.Id = id;
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

        // DELETE api/T
        [Authorize(Roles = "Admin")]
        public virtual HttpResponseMessage Delete(int id)
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
         * */
    }
}
