using System;
using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.SharedProperties
{
    public interface IHasDeletionConflicts<TConflict> where TConflict: Enum
    {
        public IEnumerable<TConflict> DeletionConflicts { get; set; }
    }
}
