﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [AllowAnonymous]
    [PublicApi]
    public class TextController : GenericApiController<Text, TextDTO>
    {
        protected readonly IGenericRepository<Text> _repository;

        public TextController(IGenericRepository<Text> repository) 
            : base(repository)
        {
            _repository = repository;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<TextDTO>>))]
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
