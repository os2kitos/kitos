using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.ApplicationServices.Model.Shared;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Model.SystemUsage.Write
{
    public class UpdatedSystemUsageRoles
    {
        public Maybe<ChangedValue<Maybe<IEnumerable<UserRolePair>>>> UserRolePairs = Maybe<ChangedValue<Maybe<IEnumerable<UserRolePair>>>>.None;
    }
}
