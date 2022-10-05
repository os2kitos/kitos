﻿module Kitos.Models.Api.Organization {
    export enum CheckConnectionError {
        InvalidCvrOnOrganization = 0,
        MissingServiceAgreement = 1,
        ExistingServiceAgreementIssue = 2,
        Unknown = 3
    }
}