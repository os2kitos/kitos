using Core.DomainModel;
using Core.DomainModel.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class UserMap : EntityMap<User>
    {
        public override void Configure(EntityTypeBuilder<User> builder)
        {
            base.Configure(builder);
            base.Configure(builder);

            // Override base required relationships - User allows null ObjectOwner/LastChangedByUser
            builder.HasOne(t => t.ObjectOwner)
                .WithMany()
                .HasForeignKey(d => d.ObjectOwnerId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.LastChangedByUser)
                .WithMany()
                .HasForeignKey(d => d.LastChangedByUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Properties
            builder.Property(t => t.Name)
                .HasMaxLength(UserConstraints.MaxNameLength)
                .IsRequired();
            builder.HasIndex(x => new { x.Name, x.LastName }).HasDatabaseName("User_Index_Name");

            builder.Property(t => t.LastName)
                .HasMaxLength(UserConstraints.MaxNameLength);

            builder.Property(t => t.Email)
                .HasMaxLength(UserConstraints.MaxEmailLength)
                .IsRequired();
            builder.HasIndex(x => x.Email).IsUnique().HasDatabaseName("User_Index_Email");

            builder.Property(t => t.Password).IsRequired();
            builder.Property(t => t.Salt).IsRequired();
            builder.Property(t => t.IsGlobalAdmin).IsRequired();

            builder.Property(x => x.Uuid).IsRequired();
            builder.HasIndex(x => x.Uuid).IsUnique().HasDatabaseName("UX_User_Uuid");

            builder.ToTable("User");

            builder.Property(t => t.IsSystemIntegrator).IsRequired();
            builder.HasIndex(x => x.IsSystemIntegrator).HasDatabaseName("IX_User_IsSystemIntegrator");
        }
    }
}
