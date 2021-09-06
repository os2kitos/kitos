using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Models.API.V2.Request.Contract
{
    public class CreateNewContractRequestDTO : ContractWriteRequestDTO
    {
        /// <summary>
        /// UUID of the organization in which the contract will be created
        /// </summary>
        [Required]
        [NonEmptyGuid]
        public Guid OrganizationUuid { get; set; }
    }
}