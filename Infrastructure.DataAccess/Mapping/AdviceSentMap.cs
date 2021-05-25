using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
