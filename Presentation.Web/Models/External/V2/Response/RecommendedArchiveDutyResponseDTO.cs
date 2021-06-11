namespace Presentation.Web.Models.External.V2.Response
{
    public class RecommendedArchiveDutyResponseDTO
    {
        public RecommendedArchiveDutyResponseDTO(string name, string id)
        {
            Name = name;
            Id = id;
        }

        /// <summary>
        /// Name for archive duty
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Archive duty identifier
        /// </summary>
        public string Id { get; }
    }
}