module Kitos.Models.ItSystem {
    export class SystemDeleteErrorMessages {
        static readonly systemInUse = "Systemet kan ikke slettes! \nDa Systemet er i anvendelse i en eller flere organisationer";
        static readonly systemDependsOnThis = "Systemet kan ikke slettes! \nDa det er markeret som overordnet system for en eller flere systemer";
        static readonly interfaceDependsOnThis = "Systemet kan ikke slettes! \nDa en det er markeret som udstillersystem for en eller flere snitflader";
        static readonly systemCannotBeDeleted = "Systemet kan ikke slettes!";
        static readonly noPermission = "Fejl! Du har ikke tilladelse!";
        static readonly notAuthorized = "Fejl! Du er ikke logget ind!";
        static readonly deleteDefault = "Fejl! Kunne ikke slette IT System!";
    }
}