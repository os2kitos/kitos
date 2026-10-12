
namespace Core.ApplicationServices.Model.HelpTexts
{
    public class HelpTextCreateParameters
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string Key { get; set; }
    }
}
