using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;
using Core.DomainServices;
using Serilog;

namespace Core.ApplicationServices.Interface.ExhibitUsage
{
    public class InterfaceExhibitUsageService : IInterfaceExhibitUsageService
    {
        private readonly IGenericRepository<ItInterfaceExhibitUsage> _itInterfaceExhibitUsageRepository;
        private readonly ILogger _logger;
        private readonly IAuthorizationContext _authorizationContext;

        public InterfaceExhibitUsageService(
            IGenericRepository<ItInterfaceExhibitUsage> itInterfaceExhibitUsageRepository,
            ILogger logger,
            IAuthorizationContext authorizationContext)
        {
            _itInterfaceExhibitUsageRepository = itInterfaceExhibitUsageRepository;
            _logger = logger;
            _authorizationContext = authorizationContext;
        }

        public Result<ItInterfaceExhibitUsage, OperationFailure> Delete(int systemUsageId, int interfaceExhibitId)
        {
            var key = ItInterfaceExhibitUsage.GetKey(systemUsageId, interfaceExhibitId);
            var interfaceExhibitUsageToBeDeleted = _itInterfaceExhibitUsageRepository.GetByKey(key);
            if (interfaceExhibitUsageToBeDeleted == null)
            {
                _logger.Error($"Could not find interface exhibit usage with key {key}");
                return Result<ItInterfaceExhibitUsage, OperationFailure>.Failure(OperationFailure.NotFound);
            }

            if (!AllowDelete(interfaceExhibitUsageToBeDeleted))
            {
                return Result<ItInterfaceExhibitUsage, OperationFailure>.Failure(OperationFailure.Forbidden);
            }

            _itInterfaceExhibitUsageRepository.Delete(interfaceExhibitUsageToBeDeleted);
            _itInterfaceExhibitUsageRepository.Save();
            return Result<ItInterfaceExhibitUsage, OperationFailure>.Success(interfaceExhibitUsageToBeDeleted);
        }

        private bool AllowDelete(ItInterfaceExhibitUsage interfaceExhibitUsageToBeDeleted)
        {
            //ExhibitUsage belongs to a contract, hence a deletion is a modification of the contract
            return interfaceExhibitUsageToBeDeleted.ItContract == null ||
                   _authorizationContext.AllowModify(interfaceExhibitUsageToBeDeleted.ItContract);
        }
    }
}
