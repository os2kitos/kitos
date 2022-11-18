using Core.DomainModel.Organization;

namespace Infrastructure.DataAccess.Mapping
{
    public class StsOrganizationConsequenceLogMap : EntityMap<StsOrganizationConsequenceLog>
    {
        public StsOrganizationConsequenceLogMap()
        {
            HasRequired(x => x.ChangeLog)
                .WithMany(x => x.ConsequenceLogs)
                .HasForeignKey(x => x.ChangeLogId);

            Property(x => x.ChangeLogId)
                .HasIndexAnnotation("UX_ChangeLogId");
        }
    }
}
