using System;
using System.ComponentModel.DataAnnotations;
using Core.DomainModel.ItContract;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.SharedProperties;

namespace Presentation.Web.Models.API.V2.Request.Contract
{
    public class CreateNewContractRequestDTO : ContractWriteRequestDTO, IHasNameExternal
    {
        /// <summary>
        /// UUID of the organization in which the contract will be created
        /// </summary>
        [Required]
        [NonEmptyGuid]
        public Guid OrganizationUuid { get; set; }

        /// <summary>
        /// Name of the contract.
        /// Constraints:
        ///     - Max length: 200 characters
        ///     - Must be unique within the organization
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [MaxLength(ItContractConstraints.MaxNameLength)]
        public string Name { get; set; }
    }
}