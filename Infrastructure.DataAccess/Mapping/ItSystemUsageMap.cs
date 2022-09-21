using Core.DomainModel.ItSystemUsage;

namespace Infrastructure.DataAccess.Mapping
{
    class ItSystemUsageMap : EntityMap<ItSystemUsage>
    {
        public ItSystemUsageMap()
        {
            // Properties
            // Table & Column Mappings
            ToTable("ItSystemUsage");

            // Relationships
            HasOptional(t => t.Reference);
            HasMany(t => t.ExternalReferences)
                .WithOptional(d => d.ItSystemUsage)
                .HasForeignKey(d => d.ItSystemUsage_Id)
                .WillCascadeOnDelete(true);

            HasRequired(t => t.Organization)
                .WithMany(t => t.ItSystemUsages);

            HasOptional(t => t.ResponsibleUsage)
                .WithOptionalPrincipal(t => t.ResponsibleItSystemUsage);

            HasRequired(t => t.ItSystem)
                .WithMany(t => t.Usages);

            HasOptional(t => t.ArchiveType)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ArchiveTypeId);

            HasOptional(t => t.SensitiveDataType)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.SensitiveDataTypeId);

            HasMany(t => t.UsedBy)
                .WithRequired(t => t.ItSystemUsage)
                .HasForeignKey(d => d.ItSystemUsageId)
                .WillCascadeOnDelete(false);

            HasOptional(t => t.MainContract)
                .WithOptionalPrincipal()
                .WillCascadeOnDelete(false);

            HasMany(t => t.Contracts)
                .WithRequired(t => t.ItSystemUsage)
                .HasForeignKey(d => d.ItSystemUsageId)
                .WillCascadeOnDelete(false);

            HasOptional(t => t.ArchiveLocation)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ArchiveLocationId);

            HasOptional(t => t.ArchiveTestLocation)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ArchiveTestLocationId);

            HasOptional(t => t.ItSystemCategories)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ItSystemCategoriesId);

            HasOptional(t => t.ArchiveSupplier)
                .WithMany(t => t.ArchiveSupplierForItSystems)
                .HasForeignKey(d => d.ArchiveSupplierId);

            HasMany(t => t.SensitiveDataLevels)
                .WithRequired(t => t.ItSystemUsage)
                .WillCascadeOnDelete(true);

            Property(x => x.Version)
                .HasMaxLength(ItSystemUsage.DefaultMaxLength)
                .HasIndexAnnotation("ItSystemUsage_Index_Version", 0);

            Property(x => x.LocalCallName)
                .HasMaxLength(ItSystemUsage.DefaultMaxLength)
                .HasIndexAnnotation("ItSystemUsage_Index_LocalCallName", 0);

            Property(x => x.LocalSystemId)
                .HasMaxLength(ItSystemUsage.LongProperyMaxLength)
                .HasIndexAnnotation("ItSystemUsage_Index_LocalSystemId", 0);

            Property(x => x.RiskSupervisionDocumentationUrlName)
                .HasMaxLength(ItSystemUsage.LinkNameMaxLength)
                .HasIndexAnnotation("ItSystemUsage_Index_RiskSupervisionDocumentationUrlName", 0);

            Property(x => x.LinkToDirectoryUrlName)
                .HasMaxLength(ItSystemUsage.LinkNameMaxLength)
                .HasIndexAnnotation("ItSystemUsage_Index_LinkToDirectoryUrlName", 0);

            Property(x => x.LifeCycleStatus)
                .HasIndexAnnotation("ItSystemUsage_Index_LifeCycleStatus", 0);

            Property(x => x.Uuid)
                .IsRequired()
                .HasUniqueIndexAnnotation("UX_ItSystemUsage_Uuid", 0);
        }
    }
}
