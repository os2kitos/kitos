using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class UserAdministrationMap : EntityTypeConfiguration<UserAdministration>
    {
        public UserAdministrationMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("UserAdministration");
            this.Property(t => t.Id).HasColumnName("Id");

            // Relationships
            this.HasRequired(t => t.ItSystem)
                .WithOptional(t => t.UserAdministration);

        }
    }
}
