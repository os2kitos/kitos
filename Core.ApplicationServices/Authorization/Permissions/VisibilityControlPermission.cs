using Core.DomainModel;

namespace Core.ApplicationServices.Authorization.Permissions
{
    public class VisibilityControlPermission : Permission
    {
        public IEntity Target { get; }

        public VisibilityControlPermission(IEntity target)
        {
            Target = target;
        }

        public override bool Accept(IPermissionVisitor permissionVisitor)
        {
            return permissionVisitor.Visit(this);
        }
    }
}
