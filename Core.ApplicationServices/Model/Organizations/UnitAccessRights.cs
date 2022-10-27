using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ApplicationServices.Model.Organizations
{
    public class UnitAccessRights
    {
        public UnitAccessRights(bool canBeRead, bool canBeModified, bool canBeDeleted)
        {
            CanBeRead = canBeRead;
            CanBeModified = canBeModified;
            CanBeDeleted = canBeDeleted;
        }

        public bool CanBeRead { get; set; }
        public bool CanBeModified { get; set; }
        public bool CanBeDeleted { get; set; }
    }
}
