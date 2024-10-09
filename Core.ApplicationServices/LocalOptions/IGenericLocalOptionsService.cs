using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel;

namespace Core.ApplicationServices.LocalOptions
{
    public interface IGenericLocalOptionsService<TLocalRole, TRight, TRole>
        where TLocalRole : LocalOptionEntity<TRole>, new()
        where TRole : OptionEntity<TRight>
    {
    }
}
