using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Repositories.Contract;
using Serilog;

namespace Core.ApplicationServices.Interface.Usage
{
    public class InterfaceUsageService : IInterfaceUsageService
    {
        private readonly IGenericRepository<ItInterfaceUsage> _interfaceUsageRepository;
        private readonly ILogger _logger;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IItContractRepository _contractRepository;

        public InterfaceUsageService(
            IGenericRepository<ItInterfaceUsage> interfaceUsageRepository,
            ILogger logger,
            IAuthorizationContext authorizationContext,
            IItContractRepository contractRepository)
        {
            _interfaceUsageRepository = interfaceUsageRepository;
            _logger = logger;
            _authorizationContext = authorizationContext;
            _contractRepository = contractRepository;
        }
        public Result<ItInterfaceUsage, OperationFailure> Create(
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
                return OperationFailure.Conflict;
            }

            var contract = _contractRepository.GetById(contractId);
            if (contract == null)
            {
                _logger.Error("Contract with id: {id} not found. Cannot create interface usage", contractId);
                return OperationFailure.BadInput;
            }

            if (!AllowCreateInterfaceUsageIn(contract))
            {
                return OperationFailure.Forbidden;
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

            return newInterfaceUsage;
        }

        private bool AllowCreateInterfaceUsageIn(ItContract contract)
        {
            // interface usages belong to contracts so authorize modification access to that
            return _authorizationContext.AllowModify(contract);
        }

        public Result<ItInterfaceUsage, OperationFailure> Delete(int systemUsageId, int systemId, int interfaceId)
        {
            var key = ItInterfaceUsage.GetKey(systemUsageId, systemId, interfaceId);
            var interfaceUsageToBeDeleted = _interfaceUsageRepository.GetByKey(key);
            if (interfaceUsageToBeDeleted == null)
            {
                _logger.Error($"Could not find interface usage with key {key}");
                return OperationFailure.NotFound;
            }

            if (!AllowDelete(interfaceUsageToBeDeleted))
            {
                return OperationFailure.Forbidden;
            }
            _interfaceUsageRepository.Delete(interfaceUsageToBeDeleted);
            _interfaceUsageRepository.Save();
            return interfaceUsageToBeDeleted;
        }

        private bool AllowDelete(ItInterfaceUsage interfaceExhibitUsageToBeDeleted)
        {
            //InterfaceUsage belongs to a contract, hence a deletion is a modification of the contract
            return interfaceExhibitUsageToBeDeleted.ItContract == null ||
                   _authorizationContext.AllowModify(interfaceExhibitUsageToBeDeleted.ItContract);
        }
    }
}
