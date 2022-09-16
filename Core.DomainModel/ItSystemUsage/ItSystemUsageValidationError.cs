namespace Core.DomainModel.ItSystemUsage
{
    public enum ItSystemUsageValidationError
    {
        StartDateNotPassed = 0,
        EndDatePassed = 1,
        NotOperationalAccordingToLifeCycle = 2,
        MainContractNotActive = 2
    }
}
