using System.Collections.Generic;

namespace Core.ApplicationServices.Model.Notification
{
    public interface IHasRecipientModels
    {
        public RecipientModel Ccs { get; set; }
        public RecipientModel Receivers { get; set; }
    }
}
