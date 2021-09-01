using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Models.API.V2.Response.System
{
    public class RecommendedArchiveDutyResponseDTO
    {
        public RecommendedArchiveDutyResponseDTO(string comment, RecommendedArchiveDutyChoice id)
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
        public RecommendedArchiveDutyChoice Id { get; }
    }
}