using System;

namespace Core.ApplicationServices.Authorization.Policies
{
    public interface IModuleCreationPolicy
    {
        bool AllowCreation(int organizationId, Type type);
    }
}
