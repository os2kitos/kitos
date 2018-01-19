using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    using Core.ApplicationServices;
    using Core.DomainModel.ItSystem;
    using Core.DomainModel.ItSystemUsage;
    using Core.DomainServices;

    public class ItSystemCategoriesController : BaseOptionController<ItSystemCategories, ItSystemUsage>
    {
        public ItSystemCategoriesController(
            IGenericRepository<ItSystemCategories> repository,
            IAuthenticationService authService)
            : base(repository, authService)
        {
            
        }
    }
}