using System;

namespace Infrastructure.Services.DataAccess
{
    public interface IEntityTypeResolver
    {
        Type Resolve(Type entityType);
    }
}
