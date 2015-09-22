using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using Core.DomainModel;

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
                .IsRequired();
            this.Property(t => t.Email)
                .HasMaxLength(100)
                .IsRequired()
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new[] {new IndexAttribute("IX_Email") {IsUnique = true}}));
            this.Property(t => t.Password)
                .IsRequired();
            this.Property(t => t.Salt)
                .IsRequired();
            this.Property(t => t.IsGlobalAdmin)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("User");
        }
    }
}
