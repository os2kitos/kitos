namespace Presentation.Web.Models.API.V1
{
    public class ValueWithOptionalRemarkDTO<T>
    {
        public T Value { get; set; }

        public string Remark { get; set; }
    }
}