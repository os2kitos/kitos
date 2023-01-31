namespace Presentation.Web.Models.API.V2.Types.Shared
{
    /// <summary>
    /// A choice defining the scope of a registration
    /// </summary>
    public enum RegistrationScopeChoice
    {
        /// <summary>
        /// The scope of the registration is local to the organization in which is was created
        /// </summary>
        Local = 0,
        /// <summary>
        /// The scope of the registration is global to KITOS and can be accessed and associated by authorized clients
        /// </summary>
        Global = 1
    }
}