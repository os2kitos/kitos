using System.ComponentModel.DataAnnotations;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;

namespace Presentation.Web.Models.API.V2.Request.System.Regular;

public class LegalPropertiesUpdateRequestDTO
{
    [MaxLength(ItSystem.MaxNameLength)]
    public string SystemName { get; set; }

    [MaxLength(Organization.MaxNameLength)]
    public string DataProcessorName { get; set; }
}