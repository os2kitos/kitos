namespace Infrastructure.DataAccess.Mapping
{
    using Core.DomainModel.ItSystem;
    using Core.DomainModel.ItSystemUsage;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class PersonalDataDeletionDeadlineTypeMap : OptionEntityMap<PersonalDataDeletionDeadlineType, ItSystemUsage>
    {
        public override void Configure(EntityTypeBuilder<PersonalDataDeletionDeadlineType> builder)
        {
            base.Configure(builder);
            builder.ToTable("PersonalDataDeletionDeadlineTypes");
        }
    }
}
