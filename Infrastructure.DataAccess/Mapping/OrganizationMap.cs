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
                .HasMaxLength(100)
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

            HasMany(m => m.Reports)
                .WithRequired(m => m.Organization)
                .HasForeignKey(m => m.OrganizationId)
                .WillCascadeOnDelete(false);

            HasOptional(o => o.ContactPerson)
                .WithOptionalDependent(c => c.Organization)
                .WillCascadeOnDelete(true);
                
        }
    }
}