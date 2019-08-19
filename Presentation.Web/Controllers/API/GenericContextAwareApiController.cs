﻿using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Access;

namespace Presentation.Web.Controllers.API
{
    public class GenericContextAwareApiController<TModel, TDto> : GenericApiController<TModel, TDto>
        where TModel : Entity, IContextAware
    {
        public GenericContextAwareApiController(IGenericRepository<TModel> repository, IAccessContext accessContext = null)
            : base(repository)
        {
        }
    }
}
