using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemRoleMap : OptionEntityMap<ItSystemRole, ItSystemRight>
    {
    }
}