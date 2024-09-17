using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Integration.Presentation.Web.Tools.Extensions
{
    public static class StringExtensions
    {
        public static string Truncate(this string s, int limit)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.Length <= limit ? s : s.Substring(0, limit);
        }
    }
}
