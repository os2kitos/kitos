using Core.DomainModel;

namespace Core.ApplicationServices.Authorization.Permissions
{
    public class CreateEntityWithVisibilityPermission : Permission
    {
        public AccessModifier Visibility { get; }
        public IEntity Target { get; }

        public CreateEntityWithVisibilityPermission(AccessModifier visibility, IEntity target)
        {
            Visibility = visibility;
            Target = target;
        }

        public override bool Accept(IPermissionVisitor permissionVisitor)
        {
            return permissionVisitor.Visit(this);
        }
    }
}
