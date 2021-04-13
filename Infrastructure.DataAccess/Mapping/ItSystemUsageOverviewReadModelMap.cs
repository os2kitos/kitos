﻿using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage.Read;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOverviewReadModelMap :  EntityTypeConfiguration<ItSystemUsageOverviewReadModel>
    {
        public ItSystemUsageOverviewReadModelMap()
        {
            Property(x => x.Name)
                .HasMaxLength(ItSystem.MaxNameLength)
                .IsRequired()
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_Name", 0);

            HasRequired(t => t.Organization)
                .WithMany(t => t.ItSystemUsageOverviewReadModels)
                .HasForeignKey(d => d.OrganizationId)
                .WillCascadeOnDelete(false);

            HasRequired(x => x.SourceEntity)
                .WithMany(x => x.OverviewReadModels)
                .HasForeignKey(x => x.SourceEntityId)
                .WillCascadeOnDelete(false);
        }
    }
}
