using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.Shared.Write;


namespace Core.ApplicationServices.Model.GDPR.Write
{
    public class UpdatedDataProcessingRegistrationRoles
    {
        public OptionalValueChange<Maybe<IEnumerable<UserRolePair>>> UserRolePairs = OptionalValueChange<Maybe<IEnumerable<UserRolePair>>>.None;
    }
}
