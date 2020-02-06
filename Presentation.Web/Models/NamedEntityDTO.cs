namespace Presentation.Web.Models
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