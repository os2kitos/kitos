using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class PasswordResetRequestMap : EntityTypeConfiguration<PasswordResetRequest>
    {
        public PasswordResetRequestMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("PasswordResetRequest");
            this.Property(t => t.Id).HasColumnName("Id");

        }
    }
}