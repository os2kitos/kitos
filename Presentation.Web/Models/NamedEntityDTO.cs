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

        public override bool Equals(object obj)
        {
            if (obj?.GetType() == typeof(NamedEntityDTO))
            {
                var namedEntity = (NamedEntityDTO) obj;
                return Id == namedEntity.Id && Name == namedEntity.Name;
            }
            return base.Equals(obj);
        }
    }
}