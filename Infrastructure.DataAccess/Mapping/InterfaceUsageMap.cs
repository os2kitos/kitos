using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    class InterfaceUsageMap : EntityTypeConfiguration<InterfaceUsage>
    {
        public InterfaceUsageMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);
            
            // Table & Column Mappings
            this.ToTable("InterfaceUsage");
            this.Property(t => t.Id).HasColumnName("Id");

            this.HasRequired(t => t.ItSystemUsage)
                .WithMany(d => d.InterfaceUsages)
                .HasForeignKey(t => t.ItSystemUsageId)
                .WillCascadeOnDelete(true);

            this.HasRequired(t => t.Interface)
                .WithMany(d => d.InterfaceUsages)
                .HasForeignKey(t => t.InterfaceId)
                .WillCascadeOnDelete(true);

            this.HasOptional(t => t.Infrastructure)
                .WithMany(d => d.InfrastructureUsage)
                .HasForeignKey(t => t.InfrastructureId);

            this.HasOptional(t => t.InterfaceCategory)
                .WithMany(d => d.References)
                .HasForeignKey(t => t.InterfaceCategoryId);

        }
    }
}
