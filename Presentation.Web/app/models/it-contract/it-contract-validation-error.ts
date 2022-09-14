module Kitos.Models.ItContract {
    export enum ItContractValidationError {
        StartDateNotPassed = 0,
        EndDatePassed = 1,
        TerminationPeriodExceeded = 2
    }
}