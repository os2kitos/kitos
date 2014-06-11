using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class UserMap : EntityMap<User>
    {
        public UserMap()
        {
            //User does NOT require an ObjectOwner!
            //Otherwise, we cannot add the first user to the system
            this.HasOptional(t => t.ObjectOwner)
                .WithMany()
                .HasForeignKey(t => t.ObjectOwnerId);

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

            this.HasOptional(t => t.CreatedIn)
                .WithMany()
                .HasForeignKey(t => t.CreatedInId);
        }
    }
}