namespace Presentation.Web.Models.API.V1
{
    public class NamedEntityWithExpirationStatusDTO : NamedEntityDTO
    {
        public bool Expired { get; set; }

        public NamedEntityWithExpirationStatusDTO(int id, string name, bool expired)
            : base(id, name)
        {
            Expired = expired;
        }
    }
}