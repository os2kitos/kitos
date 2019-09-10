using Core.DomainModel.ItSystem;
using Core.DomainServices;
using System.Linq;

namespace Core.ApplicationServices
{
    public class ItInterfaceService : IItInterfaceService
    {
        private readonly IGenericRepository<DataRow> _dataRowRepository;
        private readonly IGenericRepository<ItInterface> _repository;
        public ItInterfaceService(IGenericRepository<ItInterface> repository, IGenericRepository<DataRow> dataRowRepository)
        {
            _repository = repository;
            _dataRowRepository = dataRowRepository;
        }
        public void Delete(int id)
        {
            var dataRows = _dataRowRepository.Get(x => x.ItInterfaceId == id);
            foreach (var dataRow in dataRows)
            {
                _dataRowRepository.DeleteByKey(dataRow.Id);
            }
            _dataRowRepository.Save();

            var itInterface = _repository.Get(x => x.Id == id).FirstOrDefault();

            // delete it interface
            _repository.Delete(itInterface);
            _repository.Save();
        }
    }
}
