using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;
using Core.DomainModel.Shared;
using Core.DomainServices.Model;
using Infrastructure.Services.Types;

namespace Core.DomainServices.GDPR
{
    public class DataProcessingRegistrationReadModelUpdate : IReadModelUpdate<DataProcessingRegistration, DataProcessingRegistrationReadModel>
    {
        private readonly IGenericRepository<DataProcessingRegistrationRoleAssignmentReadModel> _roleAssignmentRepository;

        public DataProcessingRegistrationReadModelUpdate(IGenericRepository<DataProcessingRegistrationRoleAssignmentReadModel> roleAssignmentRepository)
        {
            _roleAssignmentRepository = roleAssignmentRepository;
        }

        public void Apply(DataProcessingRegistration source, DataProcessingRegistrationReadModel destination)
        {
            PatchBasicInformation(source, destination);
            PatchReference(source, destination);
            PatchRoleAssignments(source, destination);
            PatchSystems(source, destination);
            PatchDataProcessors(source, destination);
            PatchIsAgreementConcluded(source, destination);
            PatchTransferToInsecureThirdCountries(source, destination); 
            PatchDataResponsible(source, destination);
        }

        private static void PatchBasicInformation(DataProcessingRegistration source,
            DataProcessingRegistrationReadModel destination)
        {
            destination.OrganizationId = source.OrganizationId;
            destination.SourceEntityId = source.Id;
            destination.Name = source.Name;
        }

        private static void PatchDataResponsible(DataProcessingRegistration source, DataProcessingRegistrationReadModel destination)
        {
            destination.DataResponsible = source.DataResponsible == null ? "" : source.DataResponsible.Name;
            destination.DataResponsibleRemark = source.DataResponsibleRemark;
        }

        private static void PatchTransferToInsecureThirdCountries(DataProcessingRegistration source, DataProcessingRegistrationReadModel destination)
        {
            destination.TransferToInsecureThirdCountries = source.TransferToInsecureThirdCountries;
        }

        private static void PatchDataProcessors(DataProcessingRegistration source, DataProcessingRegistrationReadModel destination)
        {
            destination.DataProcessorNamesAsCsv = string.Join(", ", source.DataProcessors.Select(x => x.Name));
            destination.SubDataProcessorNamesAsCsv = string.Join(", ", source.SubDataProcessors.Select(x => x.Name));
        }

        private static void PatchIsAgreementConcluded(DataProcessingRegistration source, DataProcessingRegistrationReadModel destination)
        {
            destination.IsAgreementConcluded = source.IsAgreementConcluded;
            destination.AgreementConcludedAt = source.IsAgreementConcluded == YesNoIrrelevantOption.YES ? source.AgreementConcludedAt : null;
        }

        private static void PatchSystems(DataProcessingRegistration source, DataProcessingRegistrationReadModel destination)
        {
            destination.SystemNamesAsCsv = string.Join(", ", source.SystemUsages.Select(x => (x.ItSystem.Name, x.ItSystem.Disabled)).Select(nameStatus => $"{nameStatus.Name}{(nameStatus.Disabled ? " (Ikke aktivt)" : "")}"));
        }

        private static void PatchReference(DataProcessingRegistration source, DataProcessingRegistrationReadModel destination)
        {
            destination.MainReferenceTitle = source
                .Reference
                .FromNullable()
                .Select(x => x.Title)
                .Select(title => title.Substring(0, Math.Min(title.Length, 100)))
                .GetValueOrDefault();
            destination.MainReferenceUrl = source.Reference?.URL;
            destination.MainReferenceUserAssignedId = source.Reference?.ExternalReferenceId;
        }

        private void PatchRoleAssignments(DataProcessingRegistration source, DataProcessingRegistrationReadModel destination)
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
                    assignment = new DataProcessingRegistrationRoleAssignmentReadModel
                    {
                        Parent = destination,
                        RoleId = incomingRight.RoleId,
                        UserId = incomingRight.UserId
                    };
                    destination.RoleAssignments.Add(assignment);
                }

                var fullName = $"{incomingRight.User.Name ?? ""} {incomingRight.User.LastName ?? ""}";
                assignment.UserFullName = fullName.TrimEnd().Substring(0, Math.Min(fullName.Length, 100));
            }

            _roleAssignmentRepository.Save();
        }

        private void RemoveAssignments(DataProcessingRegistrationReadModel destination, List<DataProcessingRegistrationRoleAssignmentReadModel> assignmentsToBeRemoved)
        {
            assignmentsToBeRemoved.ForEach(assignmentToBeRemoved =>
            {
                destination.RoleAssignments.Remove(assignmentToBeRemoved);
                _roleAssignmentRepository.Delete(assignmentToBeRemoved);
            });
        }
    }
}
