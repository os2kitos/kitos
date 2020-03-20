namespace Core.ApplicationServices.SSO.State
{
    public enum FlowEvent
    {
        LoginCompleted = 0,
        UserHasValidAccessRole = 1,
        UserExists = 2,
        UserAlreadyAssociated = 3,
        UserInKnownOrganization = 4,
        UserHasRole = 5,
    }
}
