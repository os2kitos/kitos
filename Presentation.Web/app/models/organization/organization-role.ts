module Kitos.Models {
    export enum OrganizationRole {
        /**
         * Has read access to everything within the organization,
         * but not write access
         */
        User = "User" as any,
        /** Has write access to everything within the organization */
        LocalAdmin = "LocalAdmin" as any,
        /** Has write access to everything within the organization module */
        OrganizationModuleAdmin = "OrganizationModuleAdmin" as any,
        /** Has write access to everything within the project module */
        ProjectModuleAdmin = "ProjectModuleAdmin" as any,
        /** Has write access to everything within the system module */
        SystemModuleAdmin = "SystemModuleAdmin" as any,
        /** Has write access to everything within the contract module */
        ContractModuleAdmin = "ContractModuleAdmin" as any,
        /** Has write access to everything within the report module */
        ReportModuleAdmin = "ReportModuleAdmin" as any,
    }
}
