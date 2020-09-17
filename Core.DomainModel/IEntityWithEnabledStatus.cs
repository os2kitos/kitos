namespace Core.DomainModel
{
    public interface IEntityWithEnabledStatus
    {
        bool Disabled { get; set; }
    }
}
