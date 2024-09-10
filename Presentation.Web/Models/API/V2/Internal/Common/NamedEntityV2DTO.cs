namespace Presentation.Web.Models.API.V2.Internal.Common
{
    public class NamedEntityV2DTO
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public NamedEntityV2DTO()
        {
        }

        public NamedEntityV2DTO(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}