using System.Collections.Generic;
using Core.DomainModel;

namespace Core.DomainServices
{
    public interface IOrgService
    {
        IEnumerable<Organization> GetByUserId(int userId);
    }
}