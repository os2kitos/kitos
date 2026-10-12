namespace Core.ApplicationServices.Model.Authentication
{
    public class ClaimResponse
    {
        public required string Type { get; set; }
        public required string Value { get; set; }
    }
}