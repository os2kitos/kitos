using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItInterfaceUseMap : EntityTypeConfiguration<ItInterfaceUse>
    {
        public ItInterfaceUseMap()
        {
            HasKey(x => new
            {
                x.ItSystemId, x.ItInterfaceId
            });
        }
    }
}
