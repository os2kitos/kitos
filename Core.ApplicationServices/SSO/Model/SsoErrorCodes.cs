namespace Core.ApplicationServices.SSO.Model
{
    public enum SsoErrorCode
    {
        /// <summary>
        /// An unknown SSO error occurred
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// The user has not been assigned read access in STS adgangsstyring
        /// </summary>
        MissingPrivilege = 1,
        /// <summary>
        /// The user is logged in from an unknown organization and does not already exist in kitos so run-time provisioning is not possible.
        /// </summary>
        NoOrganizationAndRole = 2,
        /// <summary>
        /// The user cannot be found in STS Organisation
        /// </summary>
        UserNotFoundInSTS = 3,
        /// <summary>
        /// The user can not be auto provisioned
        /// </summary>
        UnableToCreateUserWithUnknownOrganization = 4
    }
}