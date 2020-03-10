using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.Qa.References;

namespace Infrastructure.DataAccess.Mapping
{
    public class BrokenLinkInInterfaceMap : EntityTypeConfiguration<BrokenLinkInInterface>
    {
        public BrokenLinkInInterfaceMap()
        {
            HasRequired(x => x.BrokenReferenceOrigin)
                .WithMany(x => x.BrokenLinkReports)
                .WillCascadeOnDelete(false);
        }
    }
}
