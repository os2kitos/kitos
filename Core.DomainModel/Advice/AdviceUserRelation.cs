using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;

namespace Core.DomainModel.Advice
{
    public enum RecieverType {
        ROLE,
        USER,
        CC,
        RECIEVER
    }

    public class AdviceUserRelation : Entity, IProjectModule, ISystemModule, IContractModule
    {
        /*public int? RoleId { get; set; }
        public int? UserId { get; set; }
        public virtual User User { get; set; }
        public int AviceId { get; set; }*/
        public string Name { get; set; }
        public RecieverType RecieverType { get; set; }
        public RecieverType RecpientType { get; set; }
        public int? AdviceId { get; set; }
        public virtual Advice Advice { get; set; }

    }
}
