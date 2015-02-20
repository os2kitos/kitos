namespace Core.ApplicationServices
{
    public class MoxImportError
    {
        public string SheetName { get; set; }
        public int Row { get; set; }
        public string Column { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return "Fejl i " + SheetName + ", felt " + Column + Row + ": " + Message;
        }
    }
}