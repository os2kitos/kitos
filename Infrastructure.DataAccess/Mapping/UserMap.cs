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
            this.Property(t => t.Municipality_Id)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("User");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Email).HasColumnName("Email");
            this.Property(t => t.Password).HasColumnName("Password");
            this.Property(t => t.Municipality_Id).HasColumnName("Municipality_Id");

            // Relationships
            this.HasOptional(t => t.Role)
                .WithMany(t => t.Users)
                .HasForeignKey(d => d.Role_Id);
            this.HasRequired(t => t.Municipality)
                .WithMany(t => t.Users)
                .HasForeignKey(d => d.Municipality_Id);

        }
    }
}