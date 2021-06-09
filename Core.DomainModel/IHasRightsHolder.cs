using Infrastructure.Services.Types;

namespace Core.DomainModel
{
    public interface IHasRightsHolder
    {
        Maybe<int> GetRightsHolderOrganizationId();
    }
}
