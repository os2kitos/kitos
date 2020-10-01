namespace Presentation.Web.Models
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