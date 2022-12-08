using System;

namespace Core.ApplicationServices.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ConvertToDanishFormatDateString(this DateTime dateTime)
        {
            return dateTime.ToString("dd-MM-yyyy");
        }
    }
}
