using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.Web.Models.External.V2.SharedProperties
{
    public interface IHasValidationExternal
    {
        DateTime? ValidFrom { get; set; }
        DateTime? ValidTo { get; set; }
        bool IsValid { get; set; }
    }
}
