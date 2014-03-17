using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UI.MVC4.Models
{
    public class LocaleDTO
    {
        public string Name { get; set; }
        public int Original_Id { get; set; }
    }

    public class LocaleInputDTO
    {
        public string Name { get; set; }
        public int Original_Id { get; set; }
        public int Municipality_Id { get; set; }
    }
}