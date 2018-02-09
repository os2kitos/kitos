using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel
{
    public class AttachedOption : Entity { 

        public int ObjectId { get; set; }
        public EntityType ObjectType { get; set; }
        public int OptionId { get; set; }
        public OptionType OptionType { get; set; }
    }

    public enum OptionType
    {
        REGULARPERSONALDATA,
        SENSITIVEPERSONALDATA,
        REGISTERTYPEDATA
    }
    public enum EntityType
    {
        ITSYSTEM,
        ITSYSTEMUSAGE
    }
}
