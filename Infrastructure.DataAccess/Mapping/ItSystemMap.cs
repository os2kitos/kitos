using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemMap : EntityMap<ItSystem>
    {
        public override void Configure(EntityTypeBuilder<ItSystem> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.Name).HasMaxLength(ItSystem.MaxNameLength).IsRequired();

            builder.HasIndex(x => new { x.OrganizationId, x.Name }).IsUnique().HasDatabaseName("UX_NameUniqueToOrg");
            builder.HasIndex(x => x.OrganizationId).HasDatabaseName("IX_OrganizationId");
            builder.HasIndex(x => x.Name).HasDatabaseName("IX_Name");

            builder.ToTable("ItSystem");

            builder.HasOne(t => t.Parent)
                .WithMany(d => d.Children)
                .HasForeignKey(t => t.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Organization)
                .WithMany(d => d.ItSystems)
                .HasForeignKey(t => t.OrganizationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.BelongsTo)
                .WithMany(d => d.BelongingSystems)
                .HasForeignKey(t => t.BelongsToId);

            builder.HasOne(t => t.BusinessType)
                .WithMany(t => t.References)
                .HasForeignKey(t => t.BusinessTypeId);

            builder.HasMany(t => t.ItInterfaceExhibits)
                .WithOne(t => t.ItSystem)
                .HasForeignKey(d => d.ItSystemId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(t => t.ExternalReferences)
                .WithOne(d => d.ItSystem)
                .HasForeignKey(d => d.ItSystem_Id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.Uuid).IsRequired();
            builder.HasIndex(x => x.Uuid).IsUnique().HasDatabaseName("UX_System_Uuuid");

            builder.Property(x => x.LegalName).HasMaxLength(ItSystem.MaxNameLength);
            builder.HasIndex(x => x.LegalName).HasDatabaseName("ItSystem_IX_LegalName");

            builder.Property(x => x.LegalDataProcessorName).HasMaxLength(Organization.MaxNameLength);
            builder.HasIndex(x => x.LegalDataProcessorName).HasDatabaseName("ItSystem_IX_LegalDataProcessorName");

            TypeMapping.AddIndexOnAccessModifier(builder);
        }
    }
}
