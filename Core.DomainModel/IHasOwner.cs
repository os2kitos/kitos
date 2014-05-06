namespace Core.DomainModel
{
    public interface IHasOwner
    {
        int ObjectOwnerId { get; set; }
        User ObjectOwner { get; set; }
    }
}