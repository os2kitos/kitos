namespace Presentation.Web.Extensions
{
    public static class HttpMethodIntent
    {
        public static bool IsMutation(this string method)
        {
            if (method == null)
            {
                return false;
            }
            switch (method.ToLowerInvariant())
            {
                case "post":
                case "put":
                case "patch":
                case "delete":
                    return true;
                default:
                    return false;
            }
        }

    }
}