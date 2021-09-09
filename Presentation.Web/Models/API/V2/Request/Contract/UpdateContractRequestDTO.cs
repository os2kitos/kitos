using System.ComponentModel.DataAnnotations;
using Core.DomainModel.ItContract;
using Presentation.Web.Models.API.V2.SharedProperties;

namespace Presentation.Web.Models.API.V2.Request.Contract
{
    public class UpdateContractRequestDTO : ContractWriteRequestDTO, IHasNameExternal
    {
        /// <summary>
        /// Name of the contract.
        /// Constraints:
        ///     - Max length: 200 characters
        ///     - Must be unique within the organization
        /// </summary>
        [MaxLength(ItContractConstraints.MaxNameLength)]
        public string Name { get; set; }
    }
}