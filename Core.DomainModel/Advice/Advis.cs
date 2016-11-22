using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel.Advice
{
    public enum ObjectType {
        Contract,
        Itsytem

    }
    public class Advis : Entity
    {
        public int? ObjectId { get; set; } 
        public ObjectType ObjectType{get;set;}
    }
}
