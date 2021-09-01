namespace Presentation.Web.Models.API.V1
{
    public class TaskRefSelectedDTO
    {
        public TaskRefDTO TaskRef { get; set; }
        public bool IsSelected { get; set; }
        public bool Inherited { get; set; }
    }
}
