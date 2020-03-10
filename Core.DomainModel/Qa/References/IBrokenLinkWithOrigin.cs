namespace Core.DomainModel.Qa.References
{
    public interface IBrokenLinkWithOrigin<T> : IBrokenLink
    {
        T BrokenReferenceOrigin { get; set; }
    }
}
