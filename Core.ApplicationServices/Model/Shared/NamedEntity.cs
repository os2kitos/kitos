using Core.DomainModel;

namespace Core.ApplicationServices.Model.Shared
{
    public class NamedEntity
    {
        public int Id { get; }
        public string Name { get; }

        public NamedEntity(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
