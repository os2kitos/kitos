using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel.ItContract;

namespace Infrastructure.DataAccess.Mapping
{
    public class HandoverTrialMap : EntityTypeConfiguration<HandoverTrial>
    {
        public HandoverTrialMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Name)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("HandoverTrial");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.Name).HasColumnName("Name");
        }
    }
}
