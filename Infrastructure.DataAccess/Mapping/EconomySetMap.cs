using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class EconomySetMap : EntityTypeConfiguration<EconomySet>
    {
        public EconomySetMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Table & Column Mappings
            this.ToTable("EconomySet");
            this.Property(t => t.Id).HasColumnName("Id");
        }
    }
}
