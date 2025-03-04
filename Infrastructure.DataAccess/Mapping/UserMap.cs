using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using Core.DomainModel;
using Core.DomainModel.Users;

namespace Infrastructure.DataAccess.Mapping
{
    public class UserMap : EntityMap<User>
    {
        public UserMap()
        {
            // User does NOT require an ObjectOwner!
            // Otherwise, we cannot add the first user to the system
            // this is a limitation of EF
            this.HasOptional(t => t.ObjectOwner)
                .WithMany()
                .HasForeignKey(d => d.ObjectOwnerId);
            // same as above
            this.HasOptional(t => t.LastChangedByUser)
                .WithMany()
                .HasForeignKey(d => d.LastChangedByUserId);

            // Properties
            this.Property(t => t.Name)
                .HasMaxLength(UserConstraints.MaxNameLength)
                .IsRequired()
                .HasIndexAnnotation("User_Index_Name", 0);
            this.Property(t => t.LastName)
                .HasMaxLength(UserConstraints.MaxNameLength)
                .IsOptional()
                .HasIndexAnnotation("User_Index_Name", 1);
            this.Property(t => t.Email)
                .HasMaxLength(UserConstraints.MaxEmailLength)
                .IsRequired()
                .HasUniqueIndexAnnotation("User_Index_Email", 2);
            this.Property(t => t.Password)
                .IsRequired();
            this.Property(t => t.Salt)
                .IsRequired();
            this.Property(t => t.IsGlobalAdmin)
                .IsRequired();
            this.Property(x => x.Uuid)
                .IsRequired()
                .HasUniqueIndexAnnotation("UX_User_Uuid", 0);

            // Table & Column Mappings
            this.ToTable("User");

            Property(t => t.Uuid)
                .IsRequired()
                .HasUniqueIndexAnnotation("UX_User_Uuid", 0);

            Property(t => t.IsSystemIntegrator)
                .IsRequired()
                .HasIndexAnnotation("IX_User_IsSystemIntegrator");
        }
    }
}
