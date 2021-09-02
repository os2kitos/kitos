using System.Collections.Generic;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.Shared.Write;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Model.GDPR.Write
{
    public class UpdatedDataProcessingRegistrationRoles
    {
        public OptionalValueChange<Maybe<IEnumerable<UserRolePair>>> UserRolePairs = OptionalValueChange<Maybe<IEnumerable<UserRolePair>>>.None;
    }
}
