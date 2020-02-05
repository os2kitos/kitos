namespace Core.DomainModel
{
    public interface IHasRelationshipWithOrganization
    {
        bool HasRelationshipWith(int organizationId);
    }
}
