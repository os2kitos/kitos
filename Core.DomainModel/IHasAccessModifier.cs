namespace Core.DomainModel
{
    public enum AccessModifier
    {
        Normal, Public, Private
    }

    public interface IHasAccessModifier //LOL
    {
        AccessModifier AccessModifier { get; set; }
    }
}