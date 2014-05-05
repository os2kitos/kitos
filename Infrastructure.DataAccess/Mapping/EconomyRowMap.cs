using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class EconomyRowMap : EntityTypeConfiguration<EconomyRow>
    {
        public EconomyRowMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Table & Column Mappings
            this.ToTable("EconomyRow");
            this.Property(t => t.Id).HasColumnName("Id");

            this.HasMany(t => t.Values)
                .WithRequired(d => d.EconomyRow)
                .HasForeignKey(d => d.EconomyRowId)
                .WillCascadeOnDelete(true);
        }
    }
}
