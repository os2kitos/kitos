namespace Core.DomainModel
{
    public abstract class Entity
    {
        public int Id { get; set; }

        public int ObjectOwnerId { get; set; }
        public virtual User ObjectOwner { get; set; }
    }
}