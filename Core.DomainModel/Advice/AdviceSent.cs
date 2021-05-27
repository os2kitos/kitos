using System;

namespace Core.DomainModel.Advice
{
    
    public class AdviceSent : Entity
    {
        public DateTime AdviceSentDate {get; set;}
        public int? AdviceId { get; set; }
        public virtual Advice Advice { get; set; }
    }
}
