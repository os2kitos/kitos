using System.Linq;
using Core.DomainModel;
using Core.DomainModel.GDPR;

namespace Core.DomainServices.GDPR
{
    public interface IDataProcessingAgreementRoleAssignmentService
    {
        IQueryable<User> GetUsersWhichCanBeAssignedToRole(DataProcessingAgreement target, DataProcessingAgreementRole role, IQueryable<User> candidates);
    }
}
