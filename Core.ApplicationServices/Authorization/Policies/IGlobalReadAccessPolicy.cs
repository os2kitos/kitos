using System;

namespace Core.ApplicationServices.Authorization.Policies
{
    public interface IGlobalReadAccessPolicy
    {
        bool Allow(Type type);
    }
}
