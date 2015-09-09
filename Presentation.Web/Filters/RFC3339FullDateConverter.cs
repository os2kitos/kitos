using Newtonsoft.Json.Converters;

namespace Presentation.Web.Filters
{
    /// <summary>
    /// Converts a datetime to <a href="http://tools.ietf.org/html/rfc3339#section-5.6">RFC 3339 full-date</a> format.
    /// </summary>
    /// <remarks>
    /// Note that this doesn't include the time part.
    /// </remarks>
    class Rfc3339FullDateConverter : IsoDateTimeConverter
    {
        public Rfc3339FullDateConverter()
        {
            base.DateTimeFormat = "yyyy-MM-dd";
        }
    }
}
