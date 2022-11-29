using Core.DomainModel.Organization;

namespace Infrastructure.DataAccess.Mapping
{
    public class StsOrganizationChangeLogMap : EntityMap<StsOrganizationChangeLog>
    {
        public StsOrganizationChangeLogMap()
        {
            HasRequired(x => x.StsOrganizationConnection)
                .WithMany(c => c.StsOrganizationChangeLogs)
                .HasForeignKey(x => x.StsOrganizationConnectionId)
                .WillCascadeOnDelete(true);

            Property(x => x.ResponsibleType)
                .IsRequired()
                .HasIndexAnnotation("IX_ChangeLogResponsibleType");

            Property(x => x.LogTime)
                .IsRequired()
                .HasIndexAnnotation("IX_LogTime");

            HasOptional(x => x.ResponsibleUser)
                .WithMany(x => x.StsOrganizationChangeLogs)
                .HasForeignKey(x => x.ResponsibleUserId);

            Property(x => x.ResponsibleUserId)
                .IsOptional()
                .HasIndexAnnotation("IX_ChangeLogName");
        }
    }
}
