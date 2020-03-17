using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.Qa.References;

namespace Infrastructure.DataAccess.Mapping
{
    public class BrokenLinkInExternalReferenceMap : EntityTypeConfiguration<BrokenLinkInExternalReference>
    {
        public BrokenLinkInExternalReferenceMap()
        {
            HasRequired(x => x.BrokenReferenceOrigin)
                .WithMany(x => x.BrokenLinkReports)
                .WillCascadeOnDelete(false);
        }
    }
}
