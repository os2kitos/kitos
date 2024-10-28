namespace Presentation.Web.Models.API.V2.Internal.Response.Excel
{
    public class ExcelImportErrorV2DTO
    {
        public string SheetName { get; set; }
        public int Row { get; set; }
        public string Column { get; set; }
        public string Message { get; set; }

        public string PrintableError => ToString();
    }
}