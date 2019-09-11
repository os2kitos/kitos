using System;
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

        public Result<OperationResult, ItInterfaceUsage> AssociateInContract(int systemUsageId, int systemId, int interfaceId, int? contractId, int? infrastructureId,
            bool isWishedFor)
        {
            var key = new object[] { systemUsageId, systemId, interfaceId };

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
            var createdInterfaceUsage = _interfaceUsageRepository.Insert(newInterfaceUsage);
            createdInterfaceUsage.ItContractId = contractId;
            createdInterfaceUsage.InfrastructureId = infrastructureId;
            createdInterfaceUsage.IsWishedFor = isWishedFor;
            _interfaceUsageRepository.Save();
            return Result<OperationResult, ItInterfaceUsage>.Ok(createdInterfaceUsage);
        }

        public OperationResult Delete(int systemUsageId, int systemId, int interfaceId)
        {
            var key = ItInterfaceUsage.GetKey(systemUsageId, systemId, interfaceId);
            try
            {
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
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to delete interface usage with key {key}");
                return OperationResult.UnknownError;
            }
        }
    }
}
