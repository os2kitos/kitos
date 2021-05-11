namespace Presentation.Web.Helpers
{
    public static class StringHelpers
    {
        public static bool IsExternalApiPath(this string path)
        {
            return path.Contains("api/v2");
        }
    }
}