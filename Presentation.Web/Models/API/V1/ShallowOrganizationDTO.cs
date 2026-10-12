using System.Diagnostics.CodeAnalysis;

namespace Presentation.Web.Models.API.V1
{
    public class ShallowOrganizationDTO : NamedEntityDTO
    {
        [SetsRequiredMembers]
        public ShallowOrganizationDTO(int id, string name, string cvrNumber) : base(id, name)
        {
            CvrNumber = cvrNumber;
        }

        public ShallowOrganizationDTO(string cvrNumber)
        {
            CvrNumber = cvrNumber;
        }

        public required string CvrNumber { get; set; }
    }
}