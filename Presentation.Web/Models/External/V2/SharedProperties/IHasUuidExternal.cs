using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.Web.Models.External.V2.SharedProperties
{
    public interface IHasUuidExternal
    {
        Guid Uuid { get; set; }
    }
}
