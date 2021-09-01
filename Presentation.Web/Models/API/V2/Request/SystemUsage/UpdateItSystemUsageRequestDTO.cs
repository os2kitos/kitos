namespace Presentation.Web.Models.API.V2.Request.SystemUsage
{
    public class UpdateItSystemUsageRequestDTO : BaseItSystemUsageWriteRequestDTO
    {
        public GeneralDataUpdateRequestDTO General { get; set; }
    }
}