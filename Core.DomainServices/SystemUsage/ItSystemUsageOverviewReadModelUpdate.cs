using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainServices.Model;

namespace Core.DomainServices.SystemUsage
{
    public class ItSystemUsageOverviewReadModelUpdate : IReadModelUpdate<DomainModel.ItSystemUsage.ItSystemUsage, ItSystemUsageOverviewReadModel>
    {
        public void Apply(DomainModel.ItSystemUsage.ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            destination.SourceEntityId = source.Id;
            destination.OrganizationId = source.OrganizationId;
            destination.Name = source.ItSystem.Name;
            destination.ItSystemDisabled = source.ItSystem.Disabled;
            destination.IsActive = source.IsActive;
            destination.Version = source.Version;
            destination.LocalCallName = source.LocalCallName;
            destination.LocalSystemId = source.LocalSystemId;
            destination.Uuid = source.ItSystem.Uuid;

            PatchParentSystemName(source, destination);
        }

        private static void PatchParentSystemName(DomainModel.ItSystemUsage.ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            if(source.ItSystem.Parent is null)
            {
                destination.ItSystemParentName = "";
            }
            else
            {
                destination.ItSystemParentName = source.ItSystem.Parent.Name;
            }
        }
    }
}
