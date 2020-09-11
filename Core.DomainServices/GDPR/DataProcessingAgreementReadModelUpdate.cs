using System.Collections.Generic;
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
            destination.MainReferenceTitle = source.Reference?.Title.Substring(0, 100);
            destination.MainReferenceUrl = source.Reference?.URL;
            destination.MainReferenceUserAssignedId = source.Reference?.ExternalReferenceId;
            PatchRoleAssignments(source, destination);
        }

        private void PatchRoleAssignments(DataProcessingAgreement source, DataProcessingAgreementReadModel destination)
        {
            static string CreateRoleKey(int roleId, int userId) => $"R:{roleId}U:{userId}";

            var incomingRights = source.Rights.ToDictionary(x => CreateRoleKey(x.RoleId, x.UserId));

            //Remove rights which were removed
            var assignmentsToBeRemoved =
                destination.RoleAssignments
                    .Where(x => incomingRights.ContainsKey(CreateRoleKey(x.RoleId, x.UserId)) == false).ToList();

            RemoveAssignments(destination, assignmentsToBeRemoved);

            var existingAssignments = destination.RoleAssignments.ToDictionary(x => CreateRoleKey(x.RoleId, x.UserId));
            foreach (var incomingRight in source.Rights.ToList())
            {
                if (!existingAssignments.TryGetValue(CreateRoleKey(incomingRight.RoleId, incomingRight.UserId), out var assignment))
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
                assignment.UserFullName = $"{incomingRight.User.Name} {incomingRight.User.LastName}".TrimEnd().Substring(0, 100);
            }

            _roleAssignmentRepository.Save();
        }

        private void RemoveAssignments(DataProcessingAgreementReadModel destination, List<DataProcessingAgreementRoleAssignmentReadModel> assignmentsToBeRemoved)
        {
            assignmentsToBeRemoved.ForEach(assignmentToBeRemoved =>
            {
                destination.RoleAssignments.Remove(assignmentToBeRemoved);
                _roleAssignmentRepository.Delete(assignmentToBeRemoved);
            });
        }
    }
}
