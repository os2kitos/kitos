namespace Core.DomainModel.ItContract
{
    public enum ItContractValidationError
    {
        StartDateNotPassed = 0,
        EndDatePassed = 1,
        TerminationPeriodExceeded = 2,
        InvalidParentContract = 3
    }
}
