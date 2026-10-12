using System;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;

namespace Core.ApplicationServices.Authorization
{
    public class UnauthenticatedAuthorizationContext : IAuthorizationContext
    {
       public IAuthorizationModel GetAuthorizationModel(IEntityOwnedByOrganization entity)
       {
           throw new NotImplementedException();
       }

        public CrossOrganizationDataReadAccessLevel GetCrossOrganizationReadAccess()
        {
            return CrossOrganizationDataReadAccessLevel.None;
        }

        public OrganizationDataReadAccessLevel GetOrganizationReadAccessLevel(int organizationId)
        {
            return OrganizationDataReadAccessLevel.None;
        }

        public EntityReadAccessLevel GetReadAccessLevel<T>()
        {
            return EntityReadAccessLevel.None;
        }

        public bool AllowReads(IEntity entity)
        {
            return false;
        }

        public bool AllowCreate<T>(int organizationId)
        {
            return false;
        }

        public bool AllowCreate<T>(int organizationId, IEntity entity)
        {
            return false;
        }

        public bool AllowModify(IEntity entity)
        {
            return false;
        }

        public bool AllowDelete(IEntity entity)
        {
            return false;
        }

        public bool HasPermission(Permission permission)
        {
            return false;
        }

        public bool AllowAdministerOrganizationRight(OrganizationRight right)
        {
            return false;
        }
    }
}