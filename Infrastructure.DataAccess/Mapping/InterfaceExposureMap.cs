using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    class InterfaceExposureMap : EntityTypeConfiguration<InterfaceExposure>
    {
        public InterfaceExposureMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);
            
            // Table & Column Mappings
            this.ToTable("InterfaceExposure");
            this.Property(t => t.Id).HasColumnName("Id");

            this.HasRequired(t => t.ItSystemUsage)
                .WithMany(d => d.InterfaceExposures)
                .HasForeignKey(t => t.ItSystemUsageId)
                .WillCascadeOnDelete(true);

            this.HasRequired(t => t.Interface)
                .WithMany(d => d.InterfaceLocalExposure)
                .HasForeignKey(t => t.InterfaceId)
                .WillCascadeOnDelete(true);

            this.HasOptional(t => t.ItContract)
                .WithMany(d => d.InterfaceExposures)
                .HasForeignKey(t => t.ItContractId)
                .WillCascadeOnDelete(true);
        }
    }
}
