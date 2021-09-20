using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.GDPR.Read;

namespace Core.ApplicationServices.GDPR
{
    public interface IDataProcessingRegistrationReadModelService
    {
        Result<IQueryable<DataProcessingRegistrationReadModel>, OperationError> GetByOrganizationId(int organizationId);
    }
}
