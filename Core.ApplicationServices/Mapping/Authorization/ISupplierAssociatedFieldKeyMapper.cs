using Core.DomainModel;
using System.Collections.Generic;

namespace Core.ApplicationServices.Mapping.Authorization
{
    public interface ISupplierAssociatedFieldKeyMapper
    {
        IEnumerable<string> MapParameterKeysToDomainKeys(IEnumerable<string> properties, IEntity entity);
    }
}
