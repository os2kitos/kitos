using Core.DomainModel.ItSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItInterfaceMap : EntityMap<ItInterface>
    {
        public override void Configure(EntityTypeBuilder<ItInterface> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.Version).HasMaxLength(ItInterface.MaxVersionLength);
            builder.Property(x => x.Name).HasMaxLength(ItInterface.MaxNameLength).IsRequired();
            builder.Property(x => x.ItInterfaceId).HasMaxLength(ItInterface.MaxNameLength).IsRequired();

            builder.HasIndex(x => new { x.OrganizationId, x.Name, x.ItInterfaceId }).IsUnique().HasDatabaseName("UX_NameAndVersionUniqueToOrg");
            builder.HasIndex(x => x.OrganizationId).HasDatabaseName("IX_OrganizationId");
            builder.HasIndex(x => x.Name).HasDatabaseName("IX_Name");
            builder.HasIndex(x => x.Version).HasDatabaseName("IX_Version");

            builder.ToTable("ItInterface");

            builder.HasOne(t => t.Interface)
                .WithMany(d => d.References)
                .HasForeignKey(t => t.InterfaceId);

            builder.HasOne(t => t.Organization)
                .WithMany(d => d.ItInterfaces)
                .HasForeignKey(t => t.OrganizationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            TypeMapping.AddIndexOnAccessModifier(builder);

            builder.Property(t => t.Uuid).IsRequired();
            builder.HasIndex(t => t.Uuid).IsUnique().HasDatabaseName("UX_ItInterface_Uuid");
        }
    }
}
