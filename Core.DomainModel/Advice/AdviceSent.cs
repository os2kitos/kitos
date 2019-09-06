using System;

namespace Core.DomainModel.AdviceSent
{
    
    public class AdviceSent : Entity
    {
        public DateTime AdviceSentDate {get; set;}
        public int? AdviceId { get; set; }
        public virtual Advice.Advice Advice { get; set; }
    }
}
