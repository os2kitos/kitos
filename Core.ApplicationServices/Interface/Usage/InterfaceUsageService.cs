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

        public Result<OperationResult, ItInterfaceUsage> Create(int systemUsageId, int systemId, int interfaceId)
        {
            var key = new object[] { systemUsageId, systemId, interfaceId };

            if (_interfaceUsageRepository.GetByKey(key) != null)
            {
                _logger.Error($"InterfaceUsage with key {{ " +
                              $"ItSystemUsageId: {systemUsageId}, " +
                              $"ItSystemId: {systemId}," +
                              $"ItInterfaceId: {interfaceId} }} already exists");

                //TODO: Add new operationResult: Conflict
                return Result<OperationResult, ItInterfaceUsage>.Fail(OperationResult.UnknownError);
            }

            var newInterfaceUsage = _interfaceUsageRepository.Create();
            newInterfaceUsage.ItSystemUsageId = systemUsageId;
            newInterfaceUsage.ItSystemId = systemId;
            newInterfaceUsage.ItInterfaceId = interfaceId;
            var createdInterfaceUsage = _interfaceUsageRepository.Insert(newInterfaceUsage);

            return Result<OperationResult, ItInterfaceUsage>.Ok(createdInterfaceUsage);
        }

        public Result<OperationResult, ItInterfaceUsage> Update(object[] key, int? contractId, int? infrastructureId, bool isWishedFor)
        {
            var interfaceUsage = _interfaceUsageRepository.GetByKey(key);
            if (interfaceUsage == null)
            {
                _logger.Error($"InterfaceUsage with key {key} not found");
                return Result<OperationResult, ItInterfaceUsage>.Fail(OperationResult.NotFound);
            }

            interfaceUsage.ItContractId = contractId;
            interfaceUsage.InfrastructureId = infrastructureId;
            interfaceUsage.IsWishedFor = isWishedFor;
            _interfaceUsageRepository.Save();
            return Result<OperationResult, ItInterfaceUsage>.Ok(interfaceUsage);
        }

        public Result<OperationResult, object> DeleteByKey(object[] key)
        {
            try
            {
                _interfaceUsageRepository.DeleteByKey(key);
                //TODO: Get by key, if null, return notfound, if not, delete and save
                _interfaceUsageRepository.Save();
                return Result<OperationResult, object>.Ok(null);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to delete interface usage with key {key}");
                return Result<OperationResult, object>.Fail(OperationResult.UnknownError);
            }
        }
    }
}
