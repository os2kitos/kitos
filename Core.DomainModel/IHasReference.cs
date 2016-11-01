using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel
{
    public interface IHasReferences
    {
       ICollection<ExternalReference>  ExternalReferences { get; set; }
        
    }
}
