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

            Property(x => x.Origin)
                .IsRequired()
                .HasIndexAnnotation("IX_ChangeLogOrigin");

            Property(x => x.LogTime)
                .IsRequired()
                .HasIndexAnnotation("IX_LogTime");

            HasOptional(x => x.User)
                .WithMany(x => x.StsOrganizationChangeLogs)
                .HasForeignKey(x => x.UserId);

            Property(x => x.UserId)
                .IsOptional()
                .HasIndexAnnotation("IX_ChangeLogName");
        }
    }
}
