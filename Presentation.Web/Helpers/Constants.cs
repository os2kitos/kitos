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

        public static class Excel
        {
            public const string UnitFileName = "OS2KITOS Organisationsenheder.xlsx";
            public const string UserFileName = "OS2KITOS Brugere.xlsx";
            public const string ContractsFileName = "OS2KITOS IT Kontrakter.xlsx";
            public const string ExcelFilePath = "~/Content/excel/";
        }

        public static class KLE
        {
            public const string FileNameStar = "kle-updates.csv";
            public const string DispositionType = "attachment";
            public const string MediaTypeHeaderValue = "text/csv";


            public static class Type
            {
                public const string Column = "Type";
                public const string ColumnName = "KLE Type";
            }
            public static class TaskKey
            {
                public const string Column = "TaskKey";
                public const string ColumnName = "Task Key";
            }

            public static class Description
            {
                public const string Column = "Description";
                public const string ColumnName = "Beskrivelse";
            }

            public static class Change
            {
                public const string Column = "Change";
                public const string ColumnName = "Ændring";
            }

            public static class ChangeDetails
            {
                public const string Column = "ChangeDescription";
                public const string ColumnName = "Ændringsbeskrivelse";
            }

        }
    }
}