using System;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Infrastructure.Services.DataAccess;
using DataRow = Core.DomainModel.ItSystem.DataRow;

namespace Core.ApplicationServices.Interface
{
    public class ItInterfaceService : IItInterfaceService
    {
        private readonly IGenericRepository<DataRow> _dataRowRepository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IOrganizationalUserContext _userContext;
        private readonly ITransactionManager _transactionManager;
        private readonly IGenericRepository<ItInterface> _repository;
        public ItInterfaceService(
            IGenericRepository<ItInterface> repository,
            IGenericRepository<DataRow> dataRowRepository,
            IAuthorizationContext authorizationContext,
            IOrganizationalUserContext userContext,
            ITransactionManager transactionManager)
        {
            _repository = repository;
            _dataRowRepository = dataRowRepository;
            _authorizationContext = authorizationContext;
            _userContext = userContext;
            _transactionManager = transactionManager;
        }
        public Result<ItInterface, OperationFailure> Delete(int id)
        {
            using (var transaction = _transactionManager.Begin(IsolationLevel.Serializable))
            {
                var itInterface = _repository.GetByKey(id);

                if (itInterface == null)
                {
                    return OperationFailure.NotFound;
                }

                if (!_authorizationContext.AllowDelete(itInterface))
                {
                    return OperationFailure.Forbidden;
                }

                if (itInterface.ExhibitedBy != null)
                {
                    return OperationFailure.Conflict;
                }

                var dataRows = _dataRowRepository.Get(x => x.ItInterfaceId == id);
                foreach (var dataRow in dataRows)
                {
                    _dataRowRepository.DeleteByKey(dataRow.Id);
                }
                _dataRowRepository.Save();

                // delete it interface
                _repository.DeleteWithReferencePreload(itInterface);
                _repository.Save();

                transaction.Commit();
                return itInterface;
            }
        }

        public Result<ItInterface, OperationFailure> Create(ItInterface newInterface)
        {
            if (newInterface == null)
            {
                throw new ArgumentNullException(nameof(newInterface));
            }

            if (!IsItInterfaceIdAndNameUnique(newInterface.ItInterfaceId, newInterface.Name,
                newInterface.OrganizationId))
            {
                return OperationFailure.Conflict;
            }

            var user = _userContext.UserEntity;

            newInterface.ObjectOwner = user;
            newInterface.LastChangedByUser = user;
            newInterface.ItInterfaceId = newInterface.ItInterfaceId ?? "";
            newInterface.Uuid = Guid.NewGuid();

            foreach (var dataRow in newInterface.DataRows)
            {
                dataRow.ObjectOwner = user;
                dataRow.LastChangedByUser = user;
            }

            if (!_authorizationContext.AllowCreate<ItInterface>(newInterface))
            {
                return OperationFailure.Forbidden;
            }

            var createdInterface = _repository.Insert(newInterface);
            _repository.Save();

            return createdInterface;
        }

        private bool IsItInterfaceIdAndNameUnique(string itInterfaceId, string name, int orgId)
        {
            var interfaceId = itInterfaceId ?? string.Empty;

            var system =
                _repository
                    .AsQueryable()
                    .ByOrganizationId(orgId)
                    .ByNameExact(name)
                    .Where(x => x.ItInterfaceId == interfaceId);

            return !system.Any();
        }
    }
}
