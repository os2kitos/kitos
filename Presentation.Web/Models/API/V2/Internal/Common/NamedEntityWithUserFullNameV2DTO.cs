namespace Presentation.Web.Models.API.V2.Internal.Common
{
    public class NamedEntityWithUserFullNameV2DTO : NamedEntityV2DTO
    {
        public string UserFullName { get; set; }
        public NamedEntityWithUserFullNameV2DTO(int id, string name, string userName)
            : base(id, name)
        {
            UserFullName = userName;
        }

        public NamedEntityWithUserFullNameV2DTO()
        {
            
        }
    }
}