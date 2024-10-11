

namespace Tests.Unit.Presentation.Web.Extensions
{
    public static class StringExtensions
    {
        private const int CvrMaxLength = 10;


        public static string AsCvr(this string s)
        {
            return s.Length <= CvrMaxLength ? s : s.Substring(0, CvrMaxLength);
        }
    }
}
