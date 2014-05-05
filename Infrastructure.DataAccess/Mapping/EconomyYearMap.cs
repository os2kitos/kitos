using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class EconomyYearMap : EntityTypeConfiguration<EconomyYear>
    {
        public EconomyYearMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Table & Column Mappings
            this.ToTable("EconomyYear");
            this.Property(t => t.Id).HasColumnName("Id");
        }
    }
}
