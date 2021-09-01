namespace Presentation.Web.Models.API.V1
{
    public class NamedEntityDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public NamedEntityDTO(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}