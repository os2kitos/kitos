using Core.DomainModel.GDPR;

namespace Infrastructure.DataAccess.Mapping
{
    public class DataProcessingRegistrationRightMap : RightMap<DataProcessingRegistration, DataProcessingRegistrationRight, DataProcessingRegistrationRole>
    {
        public DataProcessingRegistrationRightMap()
        {
            this.HasRequired(x => x.User)
                .WithMany(x => x.DataProcessingRegistrationRights)
                .HasForeignKey(x => x.UserId);
        }
    }
}
