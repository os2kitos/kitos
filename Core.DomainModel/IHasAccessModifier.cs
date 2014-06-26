namespace Core.DomainModel
{
    public enum AccessModifier
    {
        Normal, Public, Private
    }

    public interface IHasAccessModifier
    {
        AccessModifier AccessModifier { get; set; }
    }
}