namespace Presentation.Web.Models.API.V2.Response.Shared
{
    public class CommandPermissionResponseDTO
    {
        public required string Id { get; set; }
        public bool CanExecute { get; set; }
    }
}