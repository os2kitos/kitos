namespace Presentation.Web.Models
{
    public class TaskRefSelectedDTO
    {
        public TaskRefDTO TaskRef { get; set; }
        public bool IsSelected { get; set; }
        public bool IsLocked { get; set; }
    }
}