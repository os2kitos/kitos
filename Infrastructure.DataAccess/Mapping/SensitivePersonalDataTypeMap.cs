using Core.DomainModel.ItContract;
using Core.DomainModel.Advice;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class SensitivePersonalDataTypeMap : EntityMap<RegularPersonalDataType>
    {
        public SensitivePersonalDataTypeMap()
        {
        }
    }
}
