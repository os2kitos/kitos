using System.Collections.Generic;
using Core.DomainModel.ItSystem;

namespace Core.DomainServices.Repositories.GDPR
{
    public class SensitivePersonalDataTypeRepository : ISensitivePersonalDataTypeRepository
    {
        private readonly IGenericRepository<SensitivePersonalDataType> _sensitivePersonalDataTypeRepository;

        public SensitivePersonalDataTypeRepository(IGenericRepository<SensitivePersonalDataType> sensitivePersonalDataTypeRepository)
        {
            _sensitivePersonalDataTypeRepository = sensitivePersonalDataTypeRepository;
        }

        public IEnumerable<SensitivePersonalDataType> GetSensitivePersonalDataTypes()
        {
            return _sensitivePersonalDataTypeRepository.Get();
        }
    }
}
