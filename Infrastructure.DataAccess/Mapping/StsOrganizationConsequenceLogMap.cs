using Core.DomainModel.Organization;

namespace Infrastructure.DataAccess.Mapping
{
    public class StsOrganizationConsequenceLogMap : EntityMap<StsOrganizationConsequenceLog>
    {
        public StsOrganizationConsequenceLogMap()
        {
            HasRequired(x => x.ChangeLog)
                .WithMany(x => x.ConsequenceLogs)
                .HasForeignKey(x => x.ChangeLogId)
                .WillCascadeOnDelete(true);

            Property(x => x.Uuid)
                .IsRequired()
                .HasIndexAnnotation("UX_Consequence_Uuid");

            Property(x => x.Type)
                .IsRequired()
                .HasIndexAnnotation("UX_Consequence_Type");

            Property(x => x.Name)
                .IsRequired();

            Property(x => x.Description)
                .IsRequired();
        }
    }
}
