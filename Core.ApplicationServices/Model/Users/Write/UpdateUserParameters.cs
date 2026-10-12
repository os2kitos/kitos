using System;
using System.Collections.Generic;
using Core.ApplicationServices.Model.Shared;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Model.Users.Write
{
    public class UpdateUserParameters
    {
        public required OptionalValueChange<string> Email { get; set; }
        public required OptionalValueChange<string> FirstName { get; set; }
        public required OptionalValueChange<string> LastName { get; set; }
        public required OptionalValueChange<string> PhoneNumber { get; set; }
        public required OptionalValueChange<string> DefaultUserStartPreference { get; set; }
        public required OptionalValueChange<bool> HasApiAccess { get; set; }
        public required OptionalValueChange<bool> HasStakeHolderAccess { get; set; }
        public required OptionalValueChange<IEnumerable<OrganizationRole>> Roles { get; set; }

        public bool SendMailOnUpdate {get; set; }
        public required OptionalValueChange<Guid> DefaultOrganizationUnitUuid { get; set; }

        public bool HasOnlyRoleChanges()
        {
            return (Email.HasChange == false) &&
                   (FirstName.HasChange == false) &&
                   (LastName.HasChange == false) &&
                   (PhoneNumber.HasChange == false) &&
                   (DefaultUserStartPreference.HasChange == false) &&
                   (HasApiAccess.HasChange == false) &&
                   (HasStakeHolderAccess.HasChange == false) &&
                   (DefaultOrganizationUnitUuid.HasChange == false);
        }
    }
}
