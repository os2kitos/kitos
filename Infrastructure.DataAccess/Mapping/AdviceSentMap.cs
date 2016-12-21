using Core.DomainModel.AdviceSent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DataAccess.Mapping
{
   public class AdviceSentMap : EntityMap<AdviceSent>
    {
        public AdviceSentMap() {
            this.ToTable("AdviceSent");



        }
    }
}
