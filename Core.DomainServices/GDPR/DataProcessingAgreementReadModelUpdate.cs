using System.Linq;
using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;
using Core.DomainServices.Model;

namespace Core.DomainServices.GDPR
{
    public class DataProcessingAgreementReadModelUpdate : IReadModelUpdate<DataProcessingAgreement, DataProcessingAgreementReadModel>
    {
        private readonly IGenericRepository<DataProcessingAgreementRoleAssignmentReadModel> _roleAssignmentRepository;

        public DataProcessingAgreementReadModelUpdate(IGenericRepository<DataProcessingAgreementRoleAssignmentReadModel> roleAssignmentRepository)
        {
            _roleAssignmentRepository = roleAssignmentRepository;
        }

        public void Apply(DataProcessingAgreement source, DataProcessingAgreementReadModel destination)
        {
            destination.OrganizationId = source.OrganizationId;
            destination.SourceEntityId = source.Id;
            destination.Name = source.Name;
            PatchRoleAssignments(source, destination);
        }

        private void PatchRoleAssignments(DataProcessingAgreement source, DataProcessingAgreementReadModel destination)
        {
            var incomingRights = source.Rights.ToDictionary(x => (x.RoleId, x.UserId));
            var assignmentsToBeRemoved = destination.RoleAssignments
                .Where(x => incomingRights.ContainsKey((x.RoleId, x.UserId)) == false).ToList();

            assignmentsToBeRemoved.ForEach(assignmentToBeRemoved =>
            {
                destination.RoleAssignments.Remove(assignmentToBeRemoved);
                _roleAssignmentRepository.Delete(assignmentToBeRemoved);
            });

            var existingAssignments = destination.RoleAssignments.ToDictionary(x => (x.RoleId, x.Id));
            foreach (var incomingRight in source.Rights.ToList())
            {
                if (!existingAssignments.TryGetValue((incomingRight.RoleId, incomingRight.UserId), out var assignment))
                {
                    //Append the assignment if it is not already present
                    assignment = new DataProcessingAgreementRoleAssignmentReadModel
                    {
                        Parent = destination,
                        RoleId = incomingRight.RoleId,
                        UserId = incomingRight.UserId
                    };
                    destination.RoleAssignments.Add(assignment);
                }
                assignment.UserFullName = $"{incomingRight.User.Name} {incomingRight.User.LastName}".TrimEnd();
                assignment.RoleName = incomingRight.Role.Name;
            }
        }
    }
}
