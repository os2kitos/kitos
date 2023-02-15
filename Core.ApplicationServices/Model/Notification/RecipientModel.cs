using Core.DomainModel.Advice;

namespace Core.ApplicationServices.Model.Notification
{
    public class RecipientModel
    {
        public string Email { get; set; }

        public int? ItContractRoleId { get; set; }
        public int? ItSystemRoleId { get; set; }
        public int? DataProcessingRegistrationRoleId { get; set; }

        public RecieverType ReceiverType { get; set; }
        public RecipientType RecipientType { get; set; }
        
    }
}
