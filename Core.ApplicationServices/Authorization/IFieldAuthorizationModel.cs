using Core.DomainModel;
using System;

namespace Core.ApplicationServices.Authorization
{
    public interface IFieldAuthorizationModel
    {
        FieldPermissionsResult GetFieldPermissions(IEntityOwnedByOrganization entity, string key, Guid organizationUuid);
    }
}
