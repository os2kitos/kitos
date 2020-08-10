using Core.DomainModel;

namespace Core.ApplicationServices.Authorization.Permissions
{
    public class CreateEntityWithVisibilityPermission : Permission
    {
        public AccessModifier Visibility { get; }
        public IEntity Target { get; }
        public int OrganizationId { get; }

        public CreateEntityWithVisibilityPermission(AccessModifier visibility, IEntity target, int organizationId)
        {
            Visibility = visibility;
            Target = target;
            OrganizationId = organizationId;
        }

        public override bool Accept(IPermissionVisitor permissionVisitor)
        {
            return permissionVisitor.Visit(this);
        }
    }
}
