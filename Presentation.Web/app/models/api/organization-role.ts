module Kitos.API.Models {
    export enum OrganizationRole {
        /**
         * Has read access to everything within the organization,
         * but not write access
         */
        User,
        /** Has write access to everything within the organization */
        LocalAdmin,
        /** Has write access to everything within the organization module */
        OrganizationModuleAdmin,
        /** Has write access to everything within the project module */
        ProjectModuleAdmin,
        /** Has write access to everything within the system module */
        SystemModuleAdmin,
        /** Has write access to everything within the contract module */
        ContractModuleAdmin,
        /** Has write access to organization reports. */
        ReportModuleAdmin,
        /** Has read access only */
        ReadOnly,
        /** Has Api Access*/
        ApiAccess
    }
}
