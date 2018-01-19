using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class ItSystemCategorieController : GenericOptionApiController<ItSystemCategories, ItSystemUsage, OptionDTO>
    {
        ItSystemCategorieController(IGenericRepository<ItSystemCategories> repository)
            : base(repository) { }
    }
}