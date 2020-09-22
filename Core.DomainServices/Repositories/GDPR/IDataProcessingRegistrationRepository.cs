using System.Linq;
using Core.DomainModel.GDPR;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Repositories.GDPR
{
    public interface IDataProcessingRegistrationRepository
    {
        DataProcessingRegistration Add(DataProcessingRegistration newRegistration);
        bool DeleteById(int id);
        void Update(DataProcessingRegistration dataProcessingRegistration);
        Maybe<DataProcessingRegistration> GetById(int id);
        IQueryable<DataProcessingRegistration> GetBySystemId(int systemId);
        IQueryable<DataProcessingRegistration> Search(int organizationId, Maybe<string> exactName);
    }
}
