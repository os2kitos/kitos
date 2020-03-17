using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.Qa.References;

namespace Infrastructure.DataAccess.Mapping
{
    public class BrokenExternalReferencesReportMap : EntityTypeConfiguration<BrokenExternalReferencesReport>
    {
        public BrokenExternalReferencesReportMap()
        {
            HasMany(x => x.BrokenExternalReferences)
                .WithRequired(x => x.ParentReport)
                .WillCascadeOnDelete(true);

            HasMany(x => x.BrokenInterfaceLinks)
                .WithRequired(x => x.ParentReport)
                .WillCascadeOnDelete(true);
        }
    }
}
