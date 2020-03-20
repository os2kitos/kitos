namespace Core.ApplicationServices.SSO.State
{
    public enum FlowEvent
    {
        LoginCompleted = 0,
        UserPrivilegeVerified = 1,
        UserPrivilegeInvalid = 2,
        UserSeenBefore = 3,
        UserFirstTimeSsoVisit = 4,
        OrganizationFound = 5,
        OrganizationNotFound = 6,
        NoOrganizationAndRole = 7,
        UserHasRoleInOrganization = 8,
        UserHasNoRoleInOrganization = 9,
        RoleAssigned = 10,
        UnableToResolveUserInStsOrganisation = 11,
        ExistingSsoUserWithoutRoles = 12,

        //TODO: Remove in KITOSUDV-627: User creation flow (runtime provisioning
        UnsupportedFlow = 13,
        UnknownError = 14
    }
}
