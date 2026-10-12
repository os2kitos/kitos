using Core.DomainModel.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class OrganizationUnitMap : EntityMap<OrganizationUnit>
    {
        public override void Configure(EntityTypeBuilder<OrganizationUnit> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.LocalId).HasMaxLength(OrganizationUnit.MaxNameLength);
            builder.HasIndex(x => new { x.OrganizationId, x.LocalId }).HasDatabaseName("IX_LocalId");

            builder.ToTable("OrganizationUnit");

            builder.HasOne(o => o.Parent)
                .WithMany(p => p.Children)
                .HasForeignKey(o => o.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Organization)
                .WithMany(m => m.OrgUnits)
                .HasForeignKey(o => o.OrganizationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.Using)
                .WithOne(t => t.OrganizationUnit)
                .HasForeignKey(d => d.OrganizationUnitId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.Uuid).IsRequired();
            builder.HasIndex(x => x.Uuid).IsUnique().HasDatabaseName("UX_OrganizationUnit_UUID");

            builder.HasIndex(x => x.ExternalOriginUuid).HasDatabaseName("IX_OrganizationUnit_UUID");

            builder.Property(x => x.Origin).IsRequired();
            builder.HasIndex(x => x.Origin).HasDatabaseName("IX_OrganizationUnit_Origin");
        }
    }
}
