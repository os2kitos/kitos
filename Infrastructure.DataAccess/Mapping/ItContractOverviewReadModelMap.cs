using System;
using System.Linq.Expressions;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItContract.Read;
using Core.DomainModel.Organization;
using Core.DomainModel.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItContractOverviewReadModelMap : IEntityTypeConfiguration<ItContractOverviewReadModel>
    {
        public void Configure(EntityTypeBuilder<ItContractOverviewReadModel> builder)
        {
            builder.HasOne(t => t.Organization)
                .WithMany(t => t.ItContractOverviewReadModels)
                .HasForeignKey(d => d.OrganizationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SourceEntity)
                .WithMany(x => x.OverviewReadModels)
                .HasForeignKey(x => x.SourceEntityId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.SourceEntityUuid).IsRequired();

            builder.Property(x => x.Name).HasMaxLength(ItContractConstraints.MaxNameLength);
            builder.HasIndex(x => x.Name).HasDatabaseName("IX_Contract_Name");

            builder.HasIndex(x => x.IsActive).HasDatabaseName("IX_Contract_Active");

            builder.HasIndex(x => x.ParentContractId).HasDatabaseName("IX_ParentContract_Id");
            builder.Property(x => x.ParentContractName).HasMaxLength(ItContractConstraints.MaxNameLength);
            builder.HasIndex(x => x.ParentContractName).HasDatabaseName("IX_ParentContract_Name");
            builder.HasIndex(x => x.ParentContractUuid).HasDatabaseName("IX_ParentContract_Uuid");

            MapOptionTypeReference<CriticalityType>(builder, p => p.CriticalityId, p => p.CriticalityName, p => p.CriticalityUuid);

            builder.HasIndex(x => x.ResponsibleOrgUnitId).HasDatabaseName("IX_ResponsibleOrgUnitId");

            builder.HasIndex(x => x.SupplierId).HasDatabaseName("IX_SupplierId");
            builder.Property(x => x.SupplierName).HasMaxLength(Organization.MaxNameLength);
            builder.HasIndex(x => x.SupplierName).HasDatabaseName("IX_SupplierName");

            MapOptionTypeReference<ItContractType>(builder, p => p.ContractTypeId, p => p.ContractTypeName, p => p.ContractTypeUuid);
            MapOptionTypeReference<ItContractTemplateType>(builder, p => p.ContractTemplateId, p => p.ContractTemplateName, p => p.ContractTemplateUuid);
            MapOptionTypeReference<PurchaseFormType>(builder, p => p.PurchaseFormId, p => p.PurchaseFormName, p => p.PurchaseFormUuid);
            MapOptionTypeReference<ProcurementStrategyType>(builder, p => p.ProcurementStrategyId, p => p.ProcurementStrategyName, p => p.ProcurementStrategyUuid);

            builder.HasIndex(x => x.ProcurementPlanYear).HasDatabaseName("IX_ProcurementPlanYear");
            builder.HasIndex(x => x.ProcurementPlanQuarter).HasDatabaseName("IX_ProcurementPlanQuarter");
            builder.HasIndex(x => x.ProcurementInitiated).HasDatabaseName("IX_ProcurementInitiated");

            builder.HasMany(x => x.RoleAssignments)
                .WithOne(x => x.Parent)
                .HasForeignKey(x => x.ParentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.DataProcessingAgreements)
                .WithOne(x => x.Parent)
                .HasForeignKey(x => x.ParentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.SystemRelations)
                .WithOne(x => x.Parent)
                .HasForeignKey(x => x.ParentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.ItSystemUsages)
                .WithOne(x => x.Parent)
                .HasForeignKey(x => x.ParentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.NumberOfAssociatedSystemRelations).HasDatabaseName("IX_NumberOfAssociatedSystemRelations");

            builder.HasIndex(x => x.AccumulatedAcquisitionCost).HasDatabaseName("IX_AccumulatedAcquisitionCost");
            builder.HasIndex(x => x.AccumulatedOperationCost).HasDatabaseName("IX_AccumulatedOperationCost");
            builder.HasIndex(x => x.AccumulatedOtherCost).HasDatabaseName("IX_AccumulatedOtherCost");

            builder.Property(x => x.ExternalPaymentOrganizationUnitsCsv);
            builder.Property(x => x.InternalPaymentOrganizationUnitsCsv);
            builder.Property(x => x.ItSystemUsagesSystemUuidCsv);

            builder.HasIndex(x => x.OperationRemunerationBegunDate).HasDatabaseName("IX_OperationRemunerationBegunDate");

            MapOptionTypeReference<PaymentModelType>(builder, p => p.PaymentModelId, p => p.PaymentModelName, p => p.PaymentModelUuid);
            MapOptionTypeReference<PaymentFreqencyType>(builder, p => p.PaymentFrequencyId, p => p.PaymentFrequencyName, p => p.PaymentFrequencyUuid);

            builder.HasIndex(x => x.LatestAuditDate).HasDatabaseName("IX_LatestAuditDate");

            builder.Property(x => x.Duration).HasMaxLength(100);
            builder.HasIndex(x => x.Duration).HasDatabaseName("IX_Duration");

            MapOptionTypeReference<OptionExtendType>(builder, p => p.OptionExtendId, p => p.OptionExtendName, p => p.OptionExtendUuid);
            MapOptionTypeReference<TerminationDeadlineType>(builder, p => p.TerminationDeadlineId, p => p.TerminationDeadlineName, p => p.TerminationDeadlineUuid);

            builder.HasIndex(x => x.IrrevocableTo).HasDatabaseName("IX_IrrevocableTo");
            builder.HasIndex(x => x.TerminatedAt).HasDatabaseName("IX_TerminatedAt");

            builder.Property(x => x.LastEditedByUserName).HasMaxLength(UserConstraints.MaxNameLength);
            builder.HasIndex(x => x.LastEditedByUserName).HasDatabaseName("IX_LastEditedByUserName");

            builder.HasIndex(x => x.LastEditedByUserId).HasDatabaseName("IX_LastEditedByUserId");
            builder.HasIndex(x => x.LastEditedAtDate).HasDatabaseName("IX_LastEditedAtDate");
            builder.HasIndex(x => x.Concluded).HasDatabaseName("IX_Concluded");
            builder.HasIndex(x => x.ExpirationDate).HasDatabaseName("IX_ExpirationDate");
        }

        private static void MapOptionTypeReference<T>(
            EntityTypeBuilder<ItContractOverviewReadModel> builder,
            Expression<Func<ItContractOverviewReadModel, int?>> idExpression,
            Expression<Func<ItContractOverviewReadModel, string>> nameExpression,
            Expression<Func<ItContractOverviewReadModel, Guid?>> uuidExpression)
        {
            builder.HasIndex(GetMemberName(idExpression)).HasDatabaseName($"IX_{typeof(T).Name}_Id");
            builder.Property(nameExpression).HasMaxLength(OptionEntity<T>.MaxNameLength);
            builder.HasIndex(GetMemberName(nameExpression)).HasDatabaseName($"IX_{typeof(T).Name}_Name");
            builder.HasIndex(GetMemberName(uuidExpression)).HasDatabaseName($"IX_{typeof(T).Name}_Uuid");
        }

        private static string GetMemberName<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> expression)
        {
            if (expression.Body is MemberExpression memberExpression)
                return memberExpression.Member.Name;
            throw new ArgumentException("Expression is not a member expression");
        }
    }
}
