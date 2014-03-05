using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class ConfigMap : EntityTypeConfiguration<Config>
    {
        public ConfigMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Table & Column Mappings
            this.ToTable("Config");
            this.Property(t => t.Id).HasColumnName("Id");

            // Relationships
            this.HasRequired(t => t.Municipality)
                .WithOptional(t => t.Config);

            this.HasRequired(t => t.ItSupportModuleName)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ItSupportModuleName_Id);

            this.HasRequired(t => t.ItContractModuleName)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ItContractModuleName_Id);

            this.HasRequired(t => t.ItProjectModuleName)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ItProjectModuleName_Id);

            this.HasRequired(t => t.ItSystemModuleName)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ItSystemModuleName_Id);
        }
    }
}
