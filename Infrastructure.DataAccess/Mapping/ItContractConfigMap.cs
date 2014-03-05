using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItContractConfigMap : EntityTypeConfiguration<ItContractConfig>
    {
        public ItContractConfigMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Table & Column Mappings
            this.ToTable("ItContractCfg");
            this.Property(t => t.Id).HasColumnName("Id");

            // Relationships
            this.HasRequired(t => t.Municipality)
                .WithOptional(t => t.ItContractConfig);
        }
}