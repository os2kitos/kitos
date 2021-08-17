using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Model.SystemUsage.Write
{
    public class SystemUsageUpdateParameters
    {
        public Maybe<UpdatedSystemUsageGeneralProperties> GeneralProperties { get; set; } = Maybe<UpdatedSystemUsageGeneralProperties>.None;
        public Maybe<UpdatedSystemUsageOrganizationalUseParameters> OrganizationalUsage { get; set; } = Maybe<UpdatedSystemUsageOrganizationalUseParameters>.None;

        public Maybe<UpdatedSystemUsageRoles> Roles { get; set; } = Maybe<UpdatedSystemUsageRoles>.None;
    }
}
