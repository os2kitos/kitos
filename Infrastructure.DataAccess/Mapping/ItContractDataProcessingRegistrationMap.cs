using Core.DomainModel.ItContract;
using System.Data.Entity.ModelConfiguration;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItContractDataProcessingRegistrationMap : EntityTypeConfiguration<ItContractDataProcessingRegistration>
    {
        public ItContractDataProcessingRegistrationMap()
        {
            HasKey(x => new
            {
                x.ItContractId,
                x.DataProcessingRegistrationId
            });
        }
    }
}
