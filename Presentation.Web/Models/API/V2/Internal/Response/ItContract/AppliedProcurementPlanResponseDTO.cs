using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Internal.Response.ItContract
{
    public class AppliedProcurementPlanResponseDTO

    {

        public AppliedProcurementPlanResponseDTO(int year, int quarter)
        {
            Year = year;
            Quarter = quarter;
        }

        [Required]
        public int Year { get; set; }
        [Required]
        public int Quarter { get; set; }
    }
}