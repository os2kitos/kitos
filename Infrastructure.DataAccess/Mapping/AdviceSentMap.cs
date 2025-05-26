using Core.DomainModel.Advice;

namespace Infrastructure.DataAccess.Mapping
{
   public class AdviceSentMap : EntityMap<AdviceSent>
    {
        public AdviceSentMap() {
            this.ToTable("AdviceSent");



        }
    }
}
