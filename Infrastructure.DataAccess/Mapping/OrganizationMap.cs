using Core.DomainModel;
using Core.DomainModel.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class OrganizationMap : EntityMap<Organization>
    {
        public override void Configure(EntityTypeBuilder<Organization> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.Name).HasMaxLength(Organization.MaxNameLength);
            builder.HasIndex(x => x.Name);

            builder.Property(t => t.Cvr).HasMaxLength(10);
            builder.HasIndex(t => t.Cvr);

            builder.ToTable("Organization");

            builder.HasOne(t => t.Config)
                .WithOne(t => t.Organization)
                .HasForeignKey<Config>(c => c.Id)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(t => t.Type)
                .WithMany(t => t.Organizations)
                .HasForeignKey(t => t.TypeId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.ContactPerson)
                .WithOne(c => c.Organization)
                .HasForeignKey<Organization>("ContactPerson_Id")
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(o => o.ForeignCountryCode)
                .WithMany(c => c.References)
                .HasForeignKey(o => o.ForeignCountryCodeId);

            builder.HasMany(x => x.DataResponsibles)
                .WithOne(dr => dr.Organization)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.DataProtectionAdvisors)
                .WithOne(dr => dr.Organization)
                .OnDelete(DeleteBehavior.Cascade);

            TypeMapping.AddIndexOnAccessModifier(builder);

            builder.Ignore(o => o.OrganizationOptions);

            builder.Property(x => x.Uuid).IsRequired();
            builder.HasIndex(x => x.Uuid).IsUnique().HasDatabaseName("UX_Organization_UUID");

            builder.HasIndex(x => x.IsDefaultOrganization).HasDatabaseName("IX_DEFAULT_ORG");
        }
    }
}