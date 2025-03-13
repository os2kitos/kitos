namespace Presentation.Web.Models.API.V2.Types.Contract
{
    public enum ItContractValidationErrorChoice
    {
        StartDateNotPassed = 0,
        EndDatePassed = 1,
        TerminationPeriodExceeded = 2,
        InvalidParentContract = 3,
    }
}