module Kitos.Models.ItSystem {
    export class SystemDeleteErrorMessages {
        static readonly systemInUse = "Systemet kan ikke slettes! \nDa Systemet er i brug";
        static readonly systemDependsOnThis = "Systemet kan ikke slettes! \nDa andre systemer afhænger af dette system";
        static readonly interfaceDependsOnThis = "Systemet kan ikke slettes! \nDa en snitflade afhænger af dette system";
        static readonly systemCannotBeDeleted = "Systemet kan ikke slettes!";
        static readonly noPermission = "Fejl! Du har ikke tilladelse!";
        static readonly deleteDefault = "Fejl! Kunne ikke slette IT System!";
    }
}