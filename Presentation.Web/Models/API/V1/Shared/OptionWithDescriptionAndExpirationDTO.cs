namespace Presentation.Web.Models.API.V1.Shared
{
    public class OptionWithDescriptionAndExpirationDTO : NamedEntityWithExpirationStatusDTO
    {
        public string Description { get; set; }
        public OptionWithDescriptionAndExpirationDTO(int id, string name, bool expired, string description)
            : base(id, name, expired)
        {
            Description = description;
        }

    }
}