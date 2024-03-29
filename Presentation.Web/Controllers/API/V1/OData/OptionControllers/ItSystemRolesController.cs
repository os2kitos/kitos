﻿using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData.OptionControllers
{
    [InternalApi]
    public class ItSystemRolesController : BaseOptionController<ItSystemRole, ItSystemRight>
    {
        public ItSystemRolesController(IGenericRepository<ItSystemRole> repository)
            : base(repository)
        {
        }
    }
}
