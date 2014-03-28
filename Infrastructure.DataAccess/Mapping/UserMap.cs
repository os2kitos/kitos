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

            /* This is handled automatically??
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Email).HasColumnName("Email");
            this.Property(t => t.Password).HasColumnName("Password");
             */

            // Relationships
        }
    }
}