using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Models
{
    public class ValueOptionWithOptionalNoteDTO<T>
    {
        public T Value { get; set; }

        public string Note { get; set; }
    }
}