using System.Collections.Generic;
using Presentation.Web.Models.API.V1.Organizations;

namespace Presentation.Web.Models.API.V2.Internal.Request.OrganizationUnit
{
    public class ChangeOrganizationUnitRegistrationV2RequestDTO
    {
        public IEnumerable<int> OrganizationUnitRights { get; set; } = new List<int>();
        public IEnumerable<int> ItContractRegistrations { get; set; } = new List<int>();
        public IEnumerable<ChangePaymentRegistrationV2RequestDTO> PaymentRegistrationDetails { get; set; } = new List<ChangePaymentRegistrationV2RequestDTO>();
        public IEnumerable<int> ResponsibleSystems { get; set; } = new List<int>();
        public IEnumerable<int> RelevantSystems { get; set; } = new List<int>();
    }
}