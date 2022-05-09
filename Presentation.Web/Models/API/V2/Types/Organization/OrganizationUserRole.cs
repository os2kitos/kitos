﻿namespace Presentation.Web.Models.API.V2.Types.Organization
{
    public enum OrganizationUserRole
    {
        /// <summary>
        /// Has read access to everything within the organization,
        /// but not write access
        /// </summary>
        User = 0,
        /// <summary>
        /// Has write access to everything within the organization
        /// </summary>
        LocalAdmin = 1,
        /// <summary>
        /// Has write access to everything within the organization module
        /// </summary>
        OrganizationModuleAdmin = 2,
        /// <summary>
        /// Has write access to everything within the project module
        /// </summary>
        ProjectModuleAdmin = 3,
        /// <summary>
        /// Has write access to everything within the system module
        /// </summary>
        SystemModuleAdmin = 4,
        /// <summary>
        /// Has write access to everything within the contract module
        /// </summary>
        ContractModuleAdmin = 5,
        //NOTE: A jump from 5-7 due to old report admin
        /// <summary>
        /// Access based on the rightsholder of the entity. This is a special type of access which will override some other rights and organization-boundaries and for that reason it should not be combined with other roles.
        /// </summary>
        RightsHolderAccess = 7
    }
}