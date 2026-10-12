using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Presentation.Web.Models.API.V2.Types.Organization;

namespace Presentation.Web.Models.API.V2.Response.Organization
{
    [method: SetsRequiredMembers]
    public class OrganizationResponseDTO(
        Guid uuid,
        string name,
        string cvr,
        OrganizationType organizationType,
        bool isSupplier,
        bool disabled)
        : ShallowOrganizationResponseDTO(uuid, name, cvr)
    {
        [Required]
        public OrganizationType OrganizationType { get; } = organizationType;

        public bool IsSupplier { get; } = isSupplier;
        public bool Disabled { get; set; } = disabled;
    }
}