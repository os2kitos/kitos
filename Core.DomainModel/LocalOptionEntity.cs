using Core.DomainModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel
{
    class LocalOptionEntity : Entity
    {
        public virtual int OrganizationId { get; set; }
        public Entity OptionEntity { get; set; }
        public bool IsLocallyActive { get; set; }
        public string Description { get; set; }
    }
}
