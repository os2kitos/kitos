using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel.AdviceSent
{
    
    public class AdviceSent : Advice.Advice
    {
        public DateTime AdviceSentDate {get; set;}
    }
}
