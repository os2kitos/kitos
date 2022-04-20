module Kitos.API.Models {
    export enum OrganizationRole {
        /**
         * Has read access to everything within the organization,
         * but not write access
         */
        User = 0,
        /** Has write access to everything within the organization */
        LocalAdmin = 1,
        /** Has write access to everything within the organization module */
        OrganizationModuleAdmin = 2,
        /** Has write access to everything within the project module */
        ProjectModuleAdmin = 3,
        /** Has write access to everything within the system module */
        SystemModuleAdmin = 4,
        /** Has write access to everything within the contract module */
        ContractModuleAdmin = 5,

        //NOTE: There is a jump from 5-7 here since 6 used to be "ReportModuleAdmin"

        /**Not used as org right but is present in the enum */
        GlobalAdmin = 7,
        /* RightsHolder  */
        RightsHolderAccess = 8
    }
}
