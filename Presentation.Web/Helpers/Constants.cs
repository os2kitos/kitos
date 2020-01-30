namespace Presentation.Web.Helpers
{
    public static class Constants
    {
        public static class StatusCodeMessages
        {
            public const string ForbiddenErrorMessage = "Du har ikke rettigheder til at bruge denne funktion";
            public const string UnauthorizedErrorMessage = "Du har ikke adgang til denne funktion log ind med en bruger og prøv igen";

        }

        public static class CSRFValues
        {
            public const string CookieName = "XSRF-TOKEN";
            public const string HeaderName = "X-XSRF-TOKEN";
            public const int CookieExpirationMinutes = 10;

            public const string MissingXsrfCookieError = "Manglende xsrf cookie";
            public const string MissingXsrfHeaderError = "Manglende xsrf header";
            public const string XsrfValidationFailedError = "XSRF validering fejlede";
        }
    }
}