namespace Presentation.Web.Models
{
    public class ApiReturnDTO<T>
    {
        public string Msg { get; set; }
        public T Response { get; set; }
    }
}