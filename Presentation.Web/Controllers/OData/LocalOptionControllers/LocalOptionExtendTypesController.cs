using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    public class LocalOptionExtendTypesController : LocalOptionBaseController<LocalOptionExtendType, ItContract, OptionExtendType>
    {
        public LocalOptionExtendTypesController(IGenericRepository<LocalOptionExtendType> repository, IGenericRepository<OptionExtendType> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}
