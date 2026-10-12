using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageMap : EntityMap<ItSystemUsage>
    {
        public override void Configure(EntityTypeBuilder<ItSystemUsage> builder)
        {
            base.Configure(builder);
            builder.ToTable("ItSystemUsage");

            builder.HasMany(t => t.ExternalReferences)
                .WithOne(d => d.ItSystemUsage)
                .HasForeignKey(d => d.ItSystemUsage_Id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(t => t.Organization)
                .WithMany(t => t.ItSystemUsages)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.ResponsibleUsage)
                .WithOne(t => t.ResponsibleItSystemUsage)
                .HasForeignKey<ItSystemUsageOrgUnitUsage>("ResponsibleItSystemUsage_Id")
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.ItSystem)
                .WithMany(t => t.Usages)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.ArchiveType)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ArchiveTypeId);

            builder.HasOne(t => t.SensitiveDataType)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.SensitiveDataTypeId);

            builder.HasMany(t => t.UsedBy)
                .WithOne(t => t.ItSystemUsage)
                .HasForeignKey(d => d.ItSystemUsageId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.MainContract)
                .WithOne()
                .HasForeignKey<ItContractItSystemUsage>("ItSystemUsage_Id")
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(t => t.Contracts)
                .WithOne(t => t.ItSystemUsage)
                .HasForeignKey(d => d.ItSystemUsageId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(t => t.ArchiveLocation)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ArchiveLocationId);

            builder.HasOne(t => t.ArchiveTestLocation)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ArchiveTestLocationId);

            builder.HasOne(t => t.ItSystemCategories)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ItSystemCategoriesId);

            builder.HasOne(t => t.SystemUsageCriticalityLevel)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.SystemUsageCriticalityLevelId);

            builder.HasOne(t => t.ArchiveSupplier)
                .WithMany(t => t.ArchiveSupplierForItSystems)
                .HasForeignKey(d => d.ArchiveSupplierId);

            builder.HasMany(t => t.SensitiveDataLevels)
                .WithOne(t => t.ItSystemUsage)
                .IsRequired()
                .OnDelete(DeleteBehavior.ClientCascade);

            builder.HasMany(x => x.PersonalDataOptions)
                .WithOne(o => o.ItSystemUsage)
                .HasForeignKey(o => o.ItSystemUsageId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.Version).HasMaxLength(ItSystemUsage.DefaultMaxLength);
            builder.HasIndex(x => x.Version).HasDatabaseName("ItSystemUsage_Index_Version");

            builder.Property(x => x.LocalCallName).HasMaxLength(ItSystemUsage.DefaultMaxLength);
            builder.HasIndex(x => x.LocalCallName).HasDatabaseName("ItSystemUsage_Index_LocalCallName");

            builder.Property(x => x.LocalSystemId).HasMaxLength(ItSystemUsage.LongProperyMaxLength);
            builder.HasIndex(x => x.LocalSystemId).HasDatabaseName("ItSystemUsage_Index_LocalSystemId");

            builder.Property(x => x.RiskSupervisionDocumentationUrlName).HasMaxLength(ItSystemUsage.LinkNameMaxLength);
            builder.HasIndex(x => x.RiskSupervisionDocumentationUrlName).HasDatabaseName("ItSystemUsage_Index_RiskSupervisionDocumentationUrlName");

            builder.Property(x => x.LinkToDirectoryUrlName).HasMaxLength(ItSystemUsage.LinkNameMaxLength);
            builder.HasIndex(x => x.LinkToDirectoryUrlName).HasDatabaseName("ItSystemUsage_Index_LinkToDirectoryUrlName");

            builder.HasIndex(x => x.LifeCycleStatus).HasDatabaseName("ItSystemUsage_Index_LifeCycleStatus");

            builder.Property(x => x.CriticalityFieldsLastChanged);

            builder.Property(x => x.CriticalityFieldsLastChanged);

            builder.Property(x => x.Uuid).IsRequired();
            builder.HasIndex(x => x.Uuid).IsUnique().HasDatabaseName("UX_ItSystemUsage_Uuid");

            builder.HasIndex(x => x.ItSystemId)
                .HasDatabaseName("IX_ItSystemUsage_ItSystemId");

            // LicensingAndCodeModels is persisted as a string but exposed as a collection via the entity property
            // Ignore the navigation property to prevent EF from trying to map it as an array
            builder.Ignore(x => x.LicensingAndCodeModels);
        }
    }
}
