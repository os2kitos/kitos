using Core.DomainModel.Organization;

namespace Infrastructure.DataAccess.Mapping
{
    public class StsOrganizationConnectionMap : EntityMap<StsOrganizationConnection>
    {
        public StsOrganizationConnectionMap()
        {
            HasRequired(x => x.Organization)
                .WithOptional(x => x.StsOrganizationConnection);

            Property(x => x.Connected)
                .IsRequired()
                .HasIndexAnnotation("IX_Connected");

            Property(x => x.SubscribeToUpdates)
                .IsRequired()
                .HasIndexAnnotation("IX_Required");
        }
    }
}
