using Core.ApplicationServices.Model.Authentication;

namespace Presentation.Web.Models.API.V2.Request.Token
{
    public class ClaimResponseDTO
    {
        public ClaimResponseDTO(ClaimResponse claimResponse)
        {
            Type = claimResponse.Type;
            Value = claimResponse.Value;
        }

        public string Type { get; set; }
        public string Value { get; set; }
    }
}