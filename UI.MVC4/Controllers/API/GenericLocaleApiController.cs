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
    public abstract class GenericLocaleApiController<TModel, TOriginal> : GenericApiController<TModel, LocaleDTO>
        where TModel : Entity, ILocaleEntity<TOriginal>
    {
        protected GenericLocaleApiController(IGenericRepository<TModel> repository)
            : base(repository)
        {
        }
        
        public HttpResponseMessage Get(int organization)
        {
            try
            {
                var items = Repository.Get(locale => locale.MunicipalityId == organization).ToList();

                if (!items.Any()) return NoContent();

                return Ok(Map(items));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        // GET api/T
        public HttpResponseMessage GetSingle([FromUri] int organization, [FromUri] int original)
        {
            try
            {
                var item = Repository.Get(locale => locale.MunicipalityId == organization && locale.OriginalId == original)
                              .FirstOrDefault();

                if (item == null) return NoContent();

                return Ok((item));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}
