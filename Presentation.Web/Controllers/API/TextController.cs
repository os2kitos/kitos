using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using Presentation.Web.Models;
using Presentation.Web.Models.Exceptions;

namespace Presentation.Web.Controllers.API
{
    [AllowAnonymous]
    public class TextController : GenericApiController<Text, TextDTO>
    {
        protected readonly IGenericRepository<Text> _repository;

        public TextController(IGenericRepository<Text> repository) 
            : base(repository)
        {
            _repository = repository;
        }

        public override HttpResponseMessage GetAll([FromUri] PagingModel<Text> paging)
        {
            try
            {
                var result = _repository.AsQueryable();
                var query = Page(result, paging);
                var dtos = Map(query);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }
    }
}
