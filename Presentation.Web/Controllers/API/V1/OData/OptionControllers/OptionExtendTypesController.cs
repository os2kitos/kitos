﻿using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData.OptionControllers
{
    [InternalApi]
    public class OptionExtendTypesController : BaseOptionController<OptionExtendType, ItContract>
    {
        public OptionExtendTypesController(IGenericRepository<OptionExtendType> repository)
            : base(repository)
        {
        }
    }
}