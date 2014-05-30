using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class AdminRightMap : RightMap<Organization, AdminRight, AdminRole>
    {
    }
}