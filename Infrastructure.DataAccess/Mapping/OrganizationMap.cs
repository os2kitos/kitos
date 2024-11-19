using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using Core.DomainModel.Organization;

namespace Infrastructure.DataAccess.Mapping
{
    public class OrganizationMap : EntityMap<Organization>
    {
        public OrganizationMap()
        {
            // Properties
            Property(x => x.Name)
                .HasMaxLength(Organization.MaxNameLength)
                // http://stackoverflow.com/questions/1827063/mysql-error-key-specification-without-a-key-length
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute()));
            Property(t => t.Cvr)
                .HasMaxLength(10)
                .IsOptional()
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute()));

            // Table & Column Mappings
            ToTable("Organization");

            // Relationships
            HasOptional(t => t.Config)
                .WithRequired(t => t.Organization)
                .WillCascadeOnDelete(true);

            HasRequired(t => t.Type)
                .WithMany(t => t.Organizations)
                .HasForeignKey(t => t.TypeId)
                .WillCascadeOnDelete(false);

            HasOptional(o => o.ContactPerson)
                .WithOptionalDependent(c => c.Organization)
                .WillCascadeOnDelete(true);

            HasOptional(o => o.ForeignCountryCode)
                .WithMany(c => c.References)
                .HasForeignKey(o => o.ForeignCountryCodeId);

            HasMany(x => x.DataResponsibles)
                .WithOptional(dr => dr.Organization)
                .WillCascadeOnDelete(true);

            HasMany(x => x.DataProtectionAdvisors)
                .WithOptional(dr => dr.Organization)
                .WillCascadeOnDelete(true);

            TypeMapping.AddIndexOnAccessModifier<OrganizationMap, Organization>(this);

            Property(x => x.Uuid)
                .IsRequired()
                .HasUniqueIndexAnnotation("UX_Organization_UUID", 0);

            Property(x => x.IsDefaultOrganization)
                .IsOptional()
                .HasIndexAnnotation("IX_DEFAULT_ORG", 0);
        }
    }
}