using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Infrastructure.Attributes
{
    public class ApiNotMarkedException : Exception
    {
        public ApiNotMarkedException(string message) : base(message)
        {

        }

    }
}