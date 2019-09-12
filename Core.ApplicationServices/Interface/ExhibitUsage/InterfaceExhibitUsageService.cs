using Core.ApplicationServices.Model.Result;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Serilog;

namespace Core.ApplicationServices.Interface.ExhibitUsage
{
    public class InterfaceExhibitUsageService : IInterfaceExhibitUsageService
    {
        private readonly IGenericRepository<ItInterfaceExhibitUsage> _itInterfaceExhibitUsageRepository;
        private readonly ILogger _logger;

        public InterfaceExhibitUsageService(
            IGenericRepository<ItInterfaceExhibitUsage> itInterfaceExhibitUsageRepository,
            ILogger logger)
        {
            _itInterfaceExhibitUsageRepository = itInterfaceExhibitUsageRepository;
            _logger = logger;
        }

        //TODO: Check authorization
        public OperationResult Delete(int systemUsageId, int interfaceExhibitId)
        {
            var key = ItInterfaceExhibitUsage.GetKey(systemUsageId, interfaceExhibitId);
            var interfaceExhibitUsageToBeDeleted = _itInterfaceExhibitUsageRepository.GetByKey(key);
            if (interfaceExhibitUsageToBeDeleted == null)
            {
                _logger.Error($"Could not find interface exhibit usage with key {key}");
                return OperationResult.NotFound;
            }
            _itInterfaceExhibitUsageRepository.Delete(interfaceExhibitUsageToBeDeleted);
            _itInterfaceExhibitUsageRepository.Save();
            return OperationResult.Ok;
        }
    }
}
