using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class ResponsibleOrgUnitMap : EntityMap<ItProject>
    {
        public ResponsibleOrgUnitMap()
        {
            HasKey(x => new {x.ItProjectId, x.OrganizationId});
        }
    }
}
