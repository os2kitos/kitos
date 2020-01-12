using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;

namespace Presentation.Web.Controllers.OData
{
    [PublicApi]
    [MigratedToNewAuthorizationContext]
    public class ArchivePeriodsController : BaseEntityController<ArchivePeriod>
    {
        private readonly IItSystemUsageService _itSystemUsageService;

        public ArchivePeriodsController(IGenericRepository<ArchivePeriod> repository, IItSystemUsageService itSystemUsageService)
            : base(repository)
        {
            _itSystemUsageService = itSystemUsageService;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<ArchivePeriod>(ap => _itSystemUsageService.GetById(ap.ItSystemUsageId), base.GetCrudAuthorization());
        }
    }
}