using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;
using Core.DomainModel.Shared;
using Core.DomainServices.Mapping;
using Core.DomainServices.Model;
using Core.DomainServices.Options;


namespace Core.DomainServices.GDPR
{
    public class DataProcessingRegistrationReadModelUpdate : IReadModelUpdate<DataProcessingRegistration, DataProcessingRegistrationReadModel>
    {
        private readonly IGenericRepository<DataProcessingRegistrationRoleAssignmentReadModel> _roleAssignmentRepository;
        private readonly IOptionsService<DataProcessingRegistration, DataProcessingBasisForTransferOption> _basisForTransferService;
        private readonly IOptionsService<DataProcessingRegistration, DataProcessingDataResponsibleOption> _dataResponsibleService;
        private readonly IOptionsService<DataProcessingRegistration, DataProcessingOversightOption> _oversightOptionService;

        public DataProcessingRegistrationReadModelUpdate(
            IGenericRepository<DataProcessingRegistrationRoleAssignmentReadModel> roleAssignmentRepository,
            IOptionsService<DataProcessingRegistration, DataProcessingBasisForTransferOption> basisForTransferService,
            IOptionsService<DataProcessingRegistration, DataProcessingDataResponsibleOption> dataResponsibleService,
            IOptionsService<DataProcessingRegistration, DataProcessingOversightOption> oversightOptionService)
        {
            _roleAssignmentRepository = roleAssignmentRepository;
            _basisForTransferService = basisForTransferService;
            _dataResponsibleService = dataResponsibleService;
            _oversightOptionService = oversightOptionService;
        }

        public void Apply(DataProcessingRegistration source, DataProcessingRegistrationReadModel destination)
        {
            PatchBasicInformation(source, destination);
            PatchReference(source, destination);
            PatchRoleAssignments(source, destination);
            PatchSystems(source, destination);
            PatchOversightInterval(source, destination);
            PatchDataProcessors(source, destination);
            PatchIsAgreementConcluded(source, destination);
            PatchTransferToInsecureThirdCountries(source, destination);
            PatchDataResponsible(source, destination);
            PatchOversight(source, destination);
            PatchBasisForTransfer(source, destination);
            PatchContracts(source, destination);
            PatchActiveState(source, destination);
            PatchLastUpdateBy(source, destination);
        }

        private static void PatchBasicInformation(DataProcessingRegistration source,
            DataProcessingRegistrationReadModel destination)
        {
            destination.OrganizationId = source.OrganizationId;
            destination.SourceEntityId = source.Id;
            destination.SourceEntityUuid = source.Uuid;
            destination.Name = source.Name;
        }

        private void PatchOversight(DataProcessingRegistration source, DataProcessingRegistrationReadModel destination)
        {
            destination.OversightScheduledInspectionDate = source.OversightScheduledInspectionDate;
            PatchLatestOversightDate(source, destination);
            PatchIsOversightCompleted(source, destination);
            PatchOversightOptions(source, destination);
            PatchLatestOversightRemark(source, destination);
        }

        private void PatchOversightOptions(DataProcessingRegistration source, DataProcessingRegistrationReadModel destination)
        {
            destination.OversightOptionNamesAsCsv = string.Join(", ",
                source.OversightOptions.Select(x => GetNameOfOption(source, x, _oversightOptionService)));
        }

        private void PatchBasisForTransfer(DataProcessingRegistration source, DataProcessingRegistrationReadModel destination)
        {
            destination.BasisForTransfer = GetNameOfOption(source, source.BasisForTransfer, _basisForTransferService);
            destination.BasisForTransferUuid = source.BasisForTransfer?.Uuid;
        }

        private void PatchDataResponsible(DataProcessingRegistration source, DataProcessingRegistrationReadModel destination)
        {
            destination.DataResponsible = GetNameOfOption(source, source.DataResponsible, _dataResponsibleService);
            destination.DataResponsibleUuid = source.DataResponsible?.Uuid;
        }

        private static void PatchTransferToInsecureThirdCountries(DataProcessingRegistration source, DataProcessingRegistrationReadModel destination)
        {
            destination.TransferToInsecureThirdCountries = source.TransferToInsecureThirdCountries;
        }

        private static void PatchDataProcessors(DataProcessingRegistration source, DataProcessingRegistrationReadModel destination)
        {
            destination.DataProcessorNamesAsCsv = string.Join(", ", source.DataProcessors.Select(x => x.Name));
            destination.SubDataProcessorNamesAsCsv = string.Join(", ", source.AssignedSubDataProcessors.Select(x => x.Organization).Select(x => x.Name));
        }

        private static void PatchIsAgreementConcluded(DataProcessingRegistration source, DataProcessingRegistrationReadModel destination)
        {
            destination.IsAgreementConcluded = source.IsAgreementConcluded;
            destination.AgreementConcludedAt = source.IsAgreementConcluded == YesNoIrrelevantOption.YES ? source.AgreementConcludedAt : null;
        }

        private static void PatchSystems(DataProcessingRegistration source, DataProcessingRegistrationReadModel destination)
        {
            destination.SystemNamesAsCsv = string.Join(", ", source.SystemUsages.Select(usage => usage.MapItSystemName()));
            destination.SystemUuidsAsCsv = string.Join(", ", source.SystemUsages.Select(x => x.ItSystem.Uuid));
        }

        private static void PatchContracts(DataProcessingRegistration source, DataProcessingRegistrationReadModel destination)
        {
            destination.ContractNamesAsCsv = string.Join(", ", source.AssociatedContracts.Select(x => (x.Name)));
        }

        private static void PatchActiveState(DataProcessingRegistration source, DataProcessingRegistrationReadModel destination)
        {
            destination.IsActive = source.CheckDprValidity().Result;
            destination.ActiveAccordingToMainContract = source.IsActiveAccordingToMainContract;
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

            var incomingRights = source
                .Rights
                .GroupBy(x => CreateRoleKey(x.RoleId, x.UserId))
                .ToDictionary(x => x.Key, x => x.First());

            //Remove rights which were removed
            var assignmentsToBeRemoved =
                destination.RoleAssignments
                    .Where(x => incomingRights.ContainsKey(CreateRoleKey(x.RoleId, x.UserId)) == false).ToList();

            RemoveAssignments(destination, assignmentsToBeRemoved);

            var existingAssignments = destination
                .RoleAssignments
                .GroupBy(x => CreateRoleKey(x.RoleId, x.UserId))
                .ToDictionary(x => x.Key, x => x.First());

            foreach (var incomingRight in incomingRights.Values)
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

                var fullName = incomingRight.User.GetFullName().TrimEnd();
                assignment.UserFullName = fullName.Substring(0, Math.Min(fullName.Length, 100));
                assignment.Email = incomingRight.User.Email;
            }
        }

        private void PatchOversightInterval(DataProcessingRegistration source,
            DataProcessingRegistrationReadModel destination)
        {
            destination.OversightInterval = source.OversightInterval;
        }

        private void RemoveAssignments(DataProcessingRegistrationReadModel destination, List<DataProcessingRegistrationRoleAssignmentReadModel> assignmentsToBeRemoved)
        {
            assignmentsToBeRemoved.ForEach(assignmentToBeRemoved =>
            {
                destination.RoleAssignments.Remove(assignmentToBeRemoved);
                _roleAssignmentRepository.Delete(assignmentToBeRemoved);
            });
        }

        private static string GetNameOfOption<TOption>(
            DataProcessingRegistration parent,
            TOption optionEntity,
            IOptionsService<DataProcessingRegistration, TOption> service)
            where TOption : OptionEntity<DataProcessingRegistration>
        {
            if (optionEntity != null)
            {
                var available = service
                    .GetOption(parent.OrganizationId, optionEntity.Id)
                    .Select(x => x.available)
                    .GetValueOrFallback(false);

                return $"{optionEntity.Name}{(available ? string.Empty : " (udgået)")}";
            }

            return null;
        }

        private static void PatchIsOversightCompleted(DataProcessingRegistration source, DataProcessingRegistrationReadModel destination)
        {
            destination.IsOversightCompleted = source.IsOversightCompleted;
        }

        private static void PatchLatestOversightDate(DataProcessingRegistration source, DataProcessingRegistrationReadModel destination)
        {
            var latestOversight = source.GetLatestOversight();
            destination.LatestOversightDate = latestOversight.Select(x => x.OversightDate).GetValueOrDefault();
        }

        private static void PatchLatestOversightRemark(DataProcessingRegistration source,
            DataProcessingRegistrationReadModel destination)
        {
            var latestOversight = source.GetLatestOversight();
            destination.LatestOversightRemark = latestOversight.Select(x => x.OversightRemark).GetValueOrDefault();
        }

        private static void PatchLastUpdateBy(DataProcessingRegistration source, DataProcessingRegistrationReadModel destination)
        {
            destination.LastChangedById = source.LastChangedByUserId;
            destination.LastChangedByName = source.LastChangedByUser != null ? source.LastChangedByUser.GetFullName() : "";
            destination.LastChangedAt = source.LastChanged;
        }
    }
}
