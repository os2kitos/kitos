namespace Presentation.Web.Models.API.V2.Internal.Request.Options
{
    public class GlobalRegularOptionUpdateRequestDTO
    {
        public bool IsEnabled { get; set; }
        public string Name { get; set; }
        public bool IsObligatory { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }
    }
}