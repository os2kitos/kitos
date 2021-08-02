namespace Presentation.Web.Models.External.V2.Types.SystemUsage
{
    public enum DataSensitivityLevelChoice
    {
        None = 0,
        /// <summary>
        /// E.g. name, adress, CPR, phone#
        /// </summary>
        PersonData = 1,
        SensitiveData = 2,
        LegalData = 3
    }
}