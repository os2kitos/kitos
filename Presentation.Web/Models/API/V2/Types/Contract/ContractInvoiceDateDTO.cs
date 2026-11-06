using System;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Types.Contract
{
    public class ContractInvoiceDateDTO
    {
        /// <summary>
        /// Invoice period: 1 for Yearly, 1-2 for HalfYearly, 1-4 for Quarterly. Must be unique within the list.
        /// </summary>
        [Range(1, 4)]
        public int Period { get; set; }

        /// <summary>
        /// Full invoice date. The year does not need to match the selected invoice period.
        /// </summary>
        public DateTime Date { get; set; }
    }
}
