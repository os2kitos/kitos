using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Contracts.Write;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.ItContract;
using Core.DomainServices.Generic;
using Infrastructure.Services.DataAccess;


namespace Core.ApplicationServices.Contract.Write
{
    public class ItContractWriteService : IItContractWriteService
    {
        private readonly IItContractService _contractService;
        private readonly IEntityIdentityResolver _entityIdentityResolver;
        private readonly IOptionResolver _optionResolver;
        private readonly ITransactionManager _transactionManager;

        public ItContractWriteService(
            IItContractService contractService,
            IEntityIdentityResolver entityIdentityResolver,
            IOptionResolver optionResolver,
            ITransactionManager transactionManager)
        {
            _contractService = contractService;
            _entityIdentityResolver = entityIdentityResolver;
            _optionResolver = optionResolver;
            _transactionManager = transactionManager;
        }

        public Result<ItContract, OperationError> Create(Guid organizationUuid, ItContractModificationParameters parameters)
        {
            throw new NotImplementedException();
        }

        public Result<ItContract, OperationError> Update(Guid itContractUuid, ItContractModificationParameters parameters)
        {
            throw new NotImplementedException();
        }

        public Maybe<OperationError> Delete(Guid itContractUuid)
        {
            var dbId = _entityIdentityResolver.ResolveDbId<ItContract>(itContractUuid);

            if (dbId.IsNone)
                return new OperationError("Invalid contract uuid", OperationFailure.NotFound);

            return _contractService
                .Delete(dbId.Value)
                .Match(_ => Maybe<OperationError>.None, failure => new OperationError("Failed deleting contract", failure));
        }
    }
}
