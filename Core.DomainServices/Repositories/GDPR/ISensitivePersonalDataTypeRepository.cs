using System.Collections.Generic;
using Core.DomainModel.ItSystem;

namespace Core.DomainServices.Repositories.GDPR
{
    public interface ISensitivePersonalDataTypeRepository
    {
        IEnumerable<SensitivePersonalDataType> GetSensitivePersonalDataTypes();
    }
}
