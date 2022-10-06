using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;

namespace Core.DomainModel.Advice
{
    public enum RecieverType
    {
        CC = 2, //NOTE starts at 2 because RecipientType used to live here
        RECIEVER = 3
    }

    public enum RecipientType
    {
        ROLE = 0,
        USER = 1
    }

    public class AdviceUserRelation : Entity, ISystemModule, IContractModule
    {
        public int? AdviceId { get; set; }

        public int? ItContractRoleId { get; set; }
        public virtual ItContractRole ItContractRole { get; set; }
        public int? ItSystemRoleId { get; set; }
        public virtual ItSystemRole ItSystemRole { get; set; }
        public int? DataProcessingRegistrationRoleId { get; set; }
        public virtual DataProcessingRegistrationRole DataProcessingRegistrationRole { get; set; }

        /// <summary>
        /// Used for non-role assignments. It is a manually entered email with no coupling to kitos users.
        /// </summary>
        public string Email { get; set; }

        public RecieverType RecieverType { get; set; }
        public RecipientType RecpientType { get; set; }
        public virtual Advice Advice { get; set; }
    }
}
