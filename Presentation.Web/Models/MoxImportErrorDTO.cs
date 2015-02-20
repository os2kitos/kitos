using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Models
{
    public class MoxImportErrorDTO
    {
        public string SheetName { get; set; }
        public int Row { get; set; }
        public string Column { get; set; }
        public string Message { get; set; }

        public string PrintableError
        {
            get { return ToString(); }
        }
    }
}