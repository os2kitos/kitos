class SystemDeleteMessages {
    readonly errorMessageSystemInUse = "Systemet kan ikke slettes! Da Systemet er i brug";
    readonly errorMessageSystemDependsOnThis = "Systemet kan ikke slettes! Da andre systemer afhænger af dette system";
    readonly errorMessageInterfaceDependsOnThis = "Systemet kan ikke slettes! Da en snitflade afhænger af dette system";
    readonly errorMessageSystemCannotBeDeleted = "Systemet kan ikke slettes!";
    readonly errorMessageNoPermission = "Fejl! Du har ikke tilladelse!";
    readonly errorMessageDeleteDefault = "Fejl! Kunne ikke slette IT System!";
}

export = SystemDeleteMessages;