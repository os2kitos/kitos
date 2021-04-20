using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystemUsage.Read;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOverviewItProjectReadModelMap : EntityTypeConfiguration<ItSystemUsageOverviewItProjectReadModel>
    {
        public ItSystemUsageOverviewItProjectReadModelMap()
        {
            HasKey(x => x.Id);
            HasRequired(x => x.Parent)
                .WithMany(x => x.ItProjects)
                .HasForeignKey(x => x.ParentId)
                .WillCascadeOnDelete(true);

            Property(x => x.ItProjectName)
                .IsRequired()
                .HasMaxLength(ItProjectConstraints.MaxNameLength)
                .HasIndexAnnotation("ItSystemUsageOverviewItProjectReadModel_index_ItProjectName", 0);

            Property(x => x.ItProjectId)
                .IsRequired()
                .HasIndexAnnotation("ItSystemUsageOverviewItProjectReadModel_index_ItProjectId", 0);
        }
    }
}
