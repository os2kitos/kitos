using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class SensitiveDataTypeMap : OptionEntityMap<SensitiveDataType, ItSystemUsage>
    {
    }
}