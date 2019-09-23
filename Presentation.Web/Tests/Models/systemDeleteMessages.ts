class SystemDeleteMessages {
    static readonly errorMessageSystemInUse = "Systemet kan ikke slettes! Da Systemet er i brug";
    static readonly errorMessageSystemDependsOnThis = "Systemet kan ikke slettes! Da andre systemer afhænger af dette system";
    static readonly errorMessageInterfaceDependsOnThis = "Systemet kan ikke slettes! Da en snitflade afhænger af dette system";
    static readonly errorMessageSystemCannotBeDeleted = "Systemet kan ikke slettes!";
    static readonly errorMessageNoPermission = "Fejl! Du har ikke tilladelse!";
    static readonly errorMessageDeleteDefault = "Fejl! Kunne ikke slette IT System!";
}

export = SystemDeleteMessages;