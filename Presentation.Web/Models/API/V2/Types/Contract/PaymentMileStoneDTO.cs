using System;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Types.Contract
{
    public class PaymentMileStoneDTO
    {
        [Required(AllowEmptyStrings = false)]
        public string Title { get; set; }

        public DateTime? Expected { get; set; }
        public DateTime? Approved { get; set; }
    }
}