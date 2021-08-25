using System.Collections.Generic;
using Core.ApplicationServices.Model.Shared;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Model.SystemUsage.Write
{
    public class UpdatedSystemUsageRoles
    {
        public OptionalValueChange<Maybe<IEnumerable<UserRolePair>>> UserRolePairs = OptionalValueChange<Maybe<IEnumerable<UserRolePair>>>.None;
    }
}
