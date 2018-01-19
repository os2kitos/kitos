using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel
{
    public abstract class OptionHasChecked<T> : OptionEntity<T>
    {
        [NotMapped]
        public bool Checked { get; set; }
    }
}
