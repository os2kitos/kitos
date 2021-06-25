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
        /* Global admin - part of enum so must be here to prevent issues deserialization */
        GlobalAdmin = "GlobalAdmin" as any,
        /** Special rightsholder access bypasses regular organizational shields and gives special access to data for which it is marked as rightsholder **/
        RightsHolderAccess = "RightsHolderAccess" as any
    }
}
