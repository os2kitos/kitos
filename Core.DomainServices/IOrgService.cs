using System.Collections.Generic;
using Core.DomainModel;

namespace Core.DomainServices
{
    public interface IOrgService
    {
        ICollection<Organization> GetByUser(User user);
    }
}