using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DataAccess.Mapping
{
    using Core.DomainModel.ItContract;

    public class ItContractNotesMap : EntityMap<ItContractNotes>
    {
        public ItContractNotesMap()
        {
            this.ToTable("ItContractNotes");
        }
    }
}
