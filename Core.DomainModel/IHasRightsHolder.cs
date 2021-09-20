using Core.Abstractions.Types;


namespace Core.DomainModel
{
    public interface IHasRightsHolder
    {
        Maybe<int> GetRightsHolderOrganizationId();
    }
}
