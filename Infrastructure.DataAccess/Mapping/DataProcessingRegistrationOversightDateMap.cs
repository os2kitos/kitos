using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.GDPR;

namespace Infrastructure.DataAccess.Mapping
{
    public class DataProcessingRegistrationOversightDateMap : EntityTypeConfiguration<DataProcessingRegistrationOversightDate>
    {
        public DataProcessingRegistrationOversightDateMap()
        {
            HasKey(x => x.Id);
            HasRequired(x => x.Parent)
                .WithMany(x => x.OversightDates)
                .HasForeignKey(x => x.ParentId)
                .WillCascadeOnDelete(true);
        }
    }
}
