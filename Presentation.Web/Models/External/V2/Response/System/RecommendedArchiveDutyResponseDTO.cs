using Presentation.Web.Models.External.V2.Types;

namespace Presentation.Web.Models.External.V2.Response.System
{
    public class RecommendedArchiveDutyResponseDTO
    {
        public RecommendedArchiveDutyResponseDTO(string comment, RecommendedArchiveDuty id)
        {
            Comment = comment;
            Id = id;
        }

        /// <summary>
        /// Comment attached to the recommendation
        /// </summary>
        public string Comment { get; }

        /// <summary>
        /// Archive duty recommendation identifier
        /// </summary>
        public RecommendedArchiveDuty Id { get; }
    }
}