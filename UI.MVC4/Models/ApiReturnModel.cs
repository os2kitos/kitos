using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UI.MVC4.Models
{
    public class ApiReturnModel<T>
        where T : class
    {
        public string msg { get; set; }
        public T response { get; set; }
    }
}