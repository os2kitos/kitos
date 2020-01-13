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
        private readonly IGenericRepository<ItInterface> _repository;
        public ItInterfaceService(
            IGenericRepository<ItInterface> repository,
            IGenericRepository<DataRow> dataRowRepository,
            IAuthorizationContext authorizationContext)
        {
            _repository = repository;
            _dataRowRepository = dataRowRepository;
            _authorizationContext = authorizationContext;
        }
        public TwoTrackResult<ItInterface, OperationFailure> Delete(int id)
        {
            var itInterface = _repository.GetByKey(id);

            if (itInterface == null)
            {
                return TwoTrackResult<ItInterface, OperationFailure>.Failure(OperationFailure.NotFound);
            }

            if (!_authorizationContext.AllowDelete(itInterface))
            {
                return TwoTrackResult<ItInterface, OperationFailure>.Failure(OperationFailure.Forbidden);
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
            return TwoTrackResult<ItInterface, OperationFailure>.Success(itInterface);
        }
    }
}
