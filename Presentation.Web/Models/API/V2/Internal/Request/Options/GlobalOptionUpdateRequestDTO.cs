namespace Presentation.Web.Models.API.V2.Internal.Request.Options
{
    public class GlobalOptionUpdateRequestDTO
    {
        public bool IsEnabled { get; set; }
        public string Name { get; set; }
        public bool IsObligatory { get; set; }
        public string Description { get; set; }
    }
}