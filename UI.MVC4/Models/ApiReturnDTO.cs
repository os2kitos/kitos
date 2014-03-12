using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UI.MVC4.Models
{
    public class ApiReturnDTO<T>
        where T : class
    {
        public string Msg { get; set; }
        public T Response { get; set; }
    }
}