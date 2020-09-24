using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Models.Shared
{
    public class ValueOptionWithOptionalDateDTO<T>
    {
        public T Value { get; set; }

        public DateTime? OptionalDateValue { get; set; }
    }
}