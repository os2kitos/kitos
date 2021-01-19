using System.Linq;
using Core.DomainModel.GDPR.Read;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.GDPR
{
    public interface IDataProcessingRegistrationReadModelService
    {
        Result<IQueryable<DataProcessingRegistrationReadModel>, OperationError> GetByOrganizationId(int organizationId);
    }
}
