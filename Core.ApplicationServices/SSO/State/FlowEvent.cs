namespace Core.ApplicationServices.SSO.State
{
    public enum FlowEvent
    {
        LoginCompleted = 0,
        UserPrivilegeVerified = 1,
        UserPrivilegeInvalid = 2,
        UserSeenBefore = 3,
        UserFirstTimeVisit = 4,
        OrganizationFound = 5,
        OrganizationNotFound = 6,
        NoOrganizationAndRole = 7,
        UserHasRoleInOrganization = 8,
        UserHasNoRoleInOrganization = 9,
        RoleAssigned = 10
    }
}
