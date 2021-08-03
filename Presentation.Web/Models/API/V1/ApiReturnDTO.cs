namespace Presentation.Web.Models.API.V1
{
    public class ApiReturnDTO<T>
    {
        public string Msg { get; set; }
        public T Response { get; set; }
    }
}
