module Kitos.Models.ItSystemUsage {
    export enum ItSystemUsageValidationError {
        StartDateNotPassed = 0,
        EndDatePassed = 1,
        NotOperationalAccordingToLifeCycle = 2,
    }
}