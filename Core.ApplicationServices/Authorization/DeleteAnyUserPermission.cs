using Core.Abstractions.Types;

namespace Core.ApplicationServices.Authorization
{
    public class DeleteAnyUserPermission : Permission
    {
        public Maybe<int> OptionalOrganizationScopeId { get; }

        public DeleteAnyUserPermission(Maybe<int> optionalOrganizationScopeId)
        {
            OptionalOrganizationScopeId = optionalOrganizationScopeId;
        }

        public override bool Accept(IPermissionVisitor permissionVisitor)
        {
            return permissionVisitor.Visit(this);
        }
    }
}
