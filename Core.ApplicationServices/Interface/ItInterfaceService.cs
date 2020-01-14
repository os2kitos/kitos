using System;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Result;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.DomainServices.Model.Result;

namespace Core.ApplicationServices.Interface
{
    public class ItInterfaceService : IItInterfaceService
    {
        private readonly IGenericRepository<DataRow> _dataRowRepository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IOrganizationalUserContext _userContext;
        private readonly IGenericRepository<ItInterface> _repository;
        public ItInterfaceService(
            IGenericRepository<ItInterface> repository,
            IGenericRepository<DataRow> dataRowRepository,
            IAuthorizationContext authorizationContext,
            IOrganizationalUserContext userContext)
        {
            _repository = repository;
            _dataRowRepository = dataRowRepository;
            _authorizationContext = authorizationContext;
            _userContext = userContext;
        }
        public Result<ItInterface, OperationFailure> Delete(int id)
        {
            var itInterface = _repository.GetByKey(id);

            if (itInterface == null)
            {
                return Result<ItInterface, OperationFailure>.Failure(OperationFailure.NotFound);
            }

            if (!_authorizationContext.AllowDelete(itInterface))
            {
                return Result<ItInterface, OperationFailure>.Failure(OperationFailure.Forbidden);
            }

            if (itInterface.ExhibitedBy != null)
            {
                return Result<ItInterface, OperationFailure>.Failure(OperationFailure.Conflict);
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
            return Result<ItInterface, OperationFailure>.Success(itInterface);
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
                return Result<ItInterface, OperationFailure>.Failure(OperationFailure.Conflict);
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
                return Result<ItInterface, OperationFailure>.Failure(OperationFailure.Forbidden);
            }

            var createdInterface = _repository.Insert(newInterface);
            _repository.Save();

            return Result<ItInterface, OperationFailure>.Success(createdInterface);
        }

        private bool IsItInterfaceIdAndNameUnique(string itInterfaceId, string name, int orgId)
        {
            if (itInterfaceId == "undefined") itInterfaceId = null;
            var system = _repository.AsQueryable().Where(x => x.ItInterfaceId == (itInterfaceId ?? string.Empty) && x.Name == name && x.OrganizationId == orgId);
            return !system.Any();
        }
    }
}
