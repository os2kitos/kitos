using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class OrganizationRightMap : RightMap<OrganizationUnit, OrganizationRight, OrganizationRole>
    {
        public OrganizationRightMap()
        {
        }
    }
}