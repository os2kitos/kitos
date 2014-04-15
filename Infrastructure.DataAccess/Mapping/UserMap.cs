using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class UserMap : EntityTypeConfiguration<User>
    {
        public UserMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Name)
                .IsRequired();
            this.Property(t => t.Email)
                .IsRequired();
            this.Property(t => t.Password)
                .IsRequired();
            this.Property(t => t.Salt)
                .IsRequired();
            this.Property(t => t.IsGlobalAdmin)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("User");

            // Relationships
            this.HasOptional(t => t.DefaultOrganizationUnit)
                .WithMany(t => t.DefaultUsers)
                .HasForeignKey(d => d.DefaultOrganizationUnitId);
        }
    }
}