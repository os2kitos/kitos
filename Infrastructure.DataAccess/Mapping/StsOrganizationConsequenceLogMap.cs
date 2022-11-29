using Core.DomainModel.Organization;

namespace Infrastructure.DataAccess.Mapping
{
    public class StsOrganizationConsequenceLogMap : EntityMap<StsOrganizationConsequenceLog>
    {
        public StsOrganizationConsequenceLogMap()
        {
            HasRequired(x => x.ChangeLog)
                .WithMany(x => x.Entries)
                .HasForeignKey(x => x.ChangeLogId)
                .WillCascadeOnDelete(true);

            Property(x => x.ExternalUnitUuid)
                .IsRequired()
                .HasIndexAnnotation("IX_StsOrganizationConsequenceUuid");

            Property(x => x.Type)
                .IsRequired()
                .HasIndexAnnotation("IX_StsOrganizationConsequenceType");

            Property(x => x.Name)
                .IsRequired();

            Property(x => x.Description)
                .IsRequired();
        }
    }
}
