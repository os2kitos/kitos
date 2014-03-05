using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class MethodMap : EntityTypeConfiguration<Method>
    {
        public MethodMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Method");
            this.Property(t => t.Id).HasColumnName("Id");
        }
    }
}