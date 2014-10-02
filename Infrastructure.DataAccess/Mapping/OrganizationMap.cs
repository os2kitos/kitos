using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class OrganizationMap : EntityMap<Organization>
    {
        public OrganizationMap()
        {
            // Properties
            this.Property(x => x.Name)
                .HasMaxLength(100) // http://stackoverflow.com/questions/1827063/mysql-error-key-specification-without-a-key-length
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute()));

            // Table & Column Mappings
            this.ToTable("Organization");
            this.Property(t => t.Cvr).HasMaxLength(10).IsOptional();
            this.Property(t => t.Type).IsOptional();

            // Relationships
            this.HasOptional(t => t.Config)
                .WithRequired(t => t.Organization)
                .WillCascadeOnDelete(true);
        }
    }
}
