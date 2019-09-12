using Core.ApplicationServices.Model.Result;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Serilog;

namespace Core.ApplicationServices.Interface.Usage
{
    public class InterfaceUsageService : IInterfaceUsageService
    {
        private readonly IGenericRepository<ItInterfaceUsage> _interfaceUsageRepository;
        private readonly ILogger _logger;

        public InterfaceUsageService(
            IGenericRepository<ItInterfaceUsage> interfaceUsageRepository,
            ILogger logger)
        {
            _interfaceUsageRepository = interfaceUsageRepository;
            _logger = logger;
        }
        //TODO: Check authorization
        public Result<OperationResult, ItInterfaceUsage> Create(
            int systemUsageId,
            int systemId,
            int interfaceId,
            bool isWishedFor,
            int contractId,
            int? infrastructureId = null
            )
        {
            var key = ItInterfaceUsage.GetKey(systemUsageId, systemId, interfaceId);

            if (_interfaceUsageRepository.GetByKey(key) != null)
            {
                _logger.Error($"InterfaceUsage with key {{ " +
                              $"ItSystemUsageId: {systemUsageId}, " +
                              $"ItSystemId: {systemId}," +
                              $"ItInterfaceId: {interfaceId} }} already exists");
                return Result<OperationResult, ItInterfaceUsage>.Fail(OperationResult.Conflict);
            }

            var newInterfaceUsage = _interfaceUsageRepository.Create();
            newInterfaceUsage.ItSystemUsageId = systemUsageId;
            newInterfaceUsage.ItSystemId = systemId;
            newInterfaceUsage.ItInterfaceId = interfaceId;
            newInterfaceUsage.ItContractId = contractId;
            newInterfaceUsage.InfrastructureId = infrastructureId;
            newInterfaceUsage.IsWishedFor = isWishedFor;
            _interfaceUsageRepository.Insert(newInterfaceUsage);
            _interfaceUsageRepository.Save();

            return Result<OperationResult, ItInterfaceUsage>.Ok(newInterfaceUsage);
        }
        //TODO: Check authorization
        public OperationResult Delete(int systemUsageId, int systemId, int interfaceId)
        {
            var key = ItInterfaceUsage.GetKey(systemUsageId, systemId, interfaceId);
            var interfaceUsageToBeDeleted = _interfaceUsageRepository.GetByKey(key);
            if (interfaceUsageToBeDeleted == null)
            {
                _logger.Error($"Could not find interface usage with key {key}");
                return OperationResult.NotFound;
            }
            _interfaceUsageRepository.Delete(interfaceUsageToBeDeleted);
            _interfaceUsageRepository.Save();
            return OperationResult.Ok;
        }
    }
}
