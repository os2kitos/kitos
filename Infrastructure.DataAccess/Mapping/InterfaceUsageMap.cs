using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    class InterfaceUsageMap : EntityMap<InterfaceUsage>
    {
        public InterfaceUsageMap()
        {
            // Table & Column Mappings
            this.ToTable("InterfaceUsage");

            this.HasRequired(t => t.ItSystemUsage)
                .WithMany(d => d.InterfaceUsages)
                .HasForeignKey(t => t.ItSystemUsageId)
                .WillCascadeOnDelete(true);

            this.HasRequired(t => t.Interface)
                .WithMany(d => d.InterfaceLocalUsages)
                .HasForeignKey(t => t.InterfaceId)
                .WillCascadeOnDelete(true);

            this.HasOptional(t => t.ItContract)
                .WithMany(d => d.AssociatedInterfaceUsages)
                .HasForeignKey(t => t.ItContractId)
                .WillCascadeOnDelete(true);

            this.HasOptional(t => t.Infrastructure)
                .WithMany(d => d.InfrastructureUsage)
                .HasForeignKey(t => t.InfrastructureId);

        }
    }
}
