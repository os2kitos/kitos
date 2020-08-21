using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Repositories.Organization
{
    public interface IOrganizationUnitRepository
    {
        IEnumerable<int> GetSubTree(int orgKey, int unitKey);
    }
}
