using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class PasswordResetRequestMap : EntityMap<PasswordResetRequest>
    {
        public PasswordResetRequestMap()
        {
            // Properties
            // Table & Column Mappings
            this.ToTable("PasswordResetRequest");
            this.Property(t => t.Time).HasColumnName("Time");
            this.Property(t => t.UserId).HasColumnName("UserId");

            // Relationships
            this.HasRequired(t => t.User)
                .WithMany(t => t.PasswordResetRequests)
                .HasForeignKey(d => d.UserId);
                
        }
    }
}