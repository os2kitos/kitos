using System;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DomainEvents;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Core.DomainServices.Repositories.System;
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
        private readonly ITransactionManager _transactionManager;
        private readonly IDomainEvents _domainEvents;
        private readonly IGenericRepository<ItInterface> _repository;

        public ItInterfaceService(
            IGenericRepository<ItInterface> repository,
            IGenericRepository<DataRow> dataRowRepository,
            IItSystemRepository systemRepository,
            IAuthorizationContext authorizationContext,
            ITransactionManager transactionManager,
            IDomainEvents domainEvents)
        {
            _repository = repository;
            _dataRowRepository = dataRowRepository;
            _systemRepository = systemRepository;
            _authorizationContext = authorizationContext;
            _transactionManager = transactionManager;
            _domainEvents = domainEvents;
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

        public Result<ItInterface, OperationFailure> Create(int organizationId, string name, string interfaceId, AccessModifier? accessModifier = default)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (!IsItInterfaceIdAndNameUnique(interfaceId, name,
                organizationId))
            {
                return OperationFailure.Conflict;
            }
            var newInterface = new ItInterface
            {
                Name = name,
                OrganizationId = organizationId,
                ItInterfaceId = interfaceId ?? string.Empty,
                Uuid = Guid.NewGuid(),
                AccessModifier = accessModifier.GetValueOrDefault(AccessModifier.Public)
            };

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
