using System;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DomainEvents;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Core.DomainServices.Repositories.System;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using DataRow = Core.DomainModel.ItSystem.DataRow;

namespace Core.ApplicationServices.Interface
{
    public class ItInterfaceService : IItInterfaceService
    {
        private readonly IGenericRepository<DataRow> _dataRowRepository;
        private readonly IItSystemRepository _systemRepository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IOrganizationalUserContext _userContext;
        private readonly ITransactionManager _transactionManager;
        private readonly IDomainEvents _domainEvents;
        private readonly IOperationClock _operationClock;
        private readonly IGenericRepository<ItInterface> _repository;

        public ItInterfaceService(
            IGenericRepository<ItInterface> repository,
            IGenericRepository<DataRow> dataRowRepository,
            IItSystemRepository systemRepository,
            IAuthorizationContext authorizationContext,
            IOrganizationalUserContext userContext,
            ITransactionManager transactionManager,
            IDomainEvents domainEvents,
            IOperationClock operationClock)
        {
            _repository = repository;
            _dataRowRepository = dataRowRepository;
            _systemRepository = systemRepository;
            _authorizationContext = authorizationContext;
            _userContext = userContext;
            _transactionManager = transactionManager;
            _domainEvents = domainEvents;
            _operationClock = operationClock;
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

                var dataRows = itInterface.DataRows.ToList();
                foreach (var dataRow in dataRows)
                {
                    _dataRowRepository.DeleteByKey(dataRow.Id);
                }
                _dataRowRepository.Save();

                // delete it interface
                _domainEvents.Raise(new InterfaceDeleted(itInterface));
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

            newInterface.ItInterfaceId = newInterface.ItInterfaceId ?? "";
            newInterface.Uuid = Guid.NewGuid();

            if (!_authorizationContext.AllowCreate<ItInterface>(newInterface))
            {
                return OperationFailure.Forbidden;
            }

            var createdInterface = _repository.Insert(newInterface);
            _repository.Save();

            return createdInterface;
        }

        public Result<ItInterface, OperationFailure> ChangeExposingSystem(int interfaceId, int? newSystemId)
        {
            using (var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted))
            {
                var itInterface = _repository.GetByKey(interfaceId);

                if (itInterface == null)
                {
                    return OperationFailure.NotFound;
                }

                if (!_authorizationContext.AllowModify(itInterface))
                {
                    return OperationFailure.Forbidden;
                }

                Maybe<ItInterfaceExhibit> oldExhibit = itInterface.ExhibitedBy;
                Maybe<ItSystem> oldSystem = itInterface.ExhibitedBy?.ItSystem;
                var newSystem = Maybe<ItSystem>.None;

                if (newSystemId.HasValue)
                {
                    newSystem = _systemRepository.GetSystem(newSystemId.Value).FromNullable();
                    if (newSystem.IsNone)
                    {
                        return OperationFailure.BadInput;
                    }

                    if (!_authorizationContext.AllowReads(newSystem.Value))
                    {
                        return OperationFailure.Forbidden;
                    }
                }

                var newExhibit = itInterface.ChangeExhibitingSystem(newSystem);

                var changed = !oldExhibit.Equals(newExhibit);

                if (changed)
                {
                    _domainEvents.Raise(new ExposingSystemChanged(itInterface, oldSystem, newExhibit.Select(x => x.ItSystem)));

                    _repository.Save();
                    transaction.Commit();
                }

                return itInterface;
            }
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
