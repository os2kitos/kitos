namespace Presentation.Web.Models.API.V1
{
    public class SingleValueDTO<TValue>
    {
        public required TValue Value { get; set; }
    }
}