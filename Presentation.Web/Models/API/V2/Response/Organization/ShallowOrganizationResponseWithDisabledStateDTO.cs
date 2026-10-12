using System;
using System.Diagnostics.CodeAnalysis;

namespace Presentation.Web.Models.API.V2.Response.Organization;

[method: SetsRequiredMembers]
public class ShallowOrganizationResponseWithDisabledStateDTO(Guid uuid, string name, string cvr, bool disabled)
    : ShallowOrganizationResponseDTO(uuid, name, cvr)
{
    /// <summary>
    /// Indicates whether the organization is disabled or not.
    /// </summary>
    public bool Disabled { get; set; } = disabled;
}
