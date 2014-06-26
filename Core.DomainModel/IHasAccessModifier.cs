namespace Core.DomainModel
{
    public enum AccessModifier
    {
        Normal, Public, Private
    }

    /// <summary>
    /// Implemented by types whose read access can be modified
    /// </summary>
    public interface IHasAccessModifier
    {
        AccessModifier AccessModifier { get; set; }
    }
}