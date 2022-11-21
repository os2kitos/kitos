using Core.DomainModel.Organization;

namespace Infrastructure.DataAccess.Mapping
{
    public class StsOrganizationChangeLogMap : EntityMap<StsOrganizationChangeLog>
    {
        public StsOrganizationChangeLogMap()
        {
            HasRequired(x => x.StsOrganizationConnection)
                .WithMany(c => c.StsOrganizationChangeLogs)
                .HasForeignKey(x => x.ConnectionId)
                .WillCascadeOnDelete(true);

            Property(x => x.Origin)
                .IsRequired()
                .HasIndexAnnotation("UX_ChangeLogOrigin");

            Property(x => x.LogTime)
                .IsRequired()
                .HasIndexAnnotation("UX_LogTime");

            Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired()
                .HasIndexAnnotation("UX_ChangeLogName");
        }
    }
}
