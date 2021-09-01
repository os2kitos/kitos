namespace Presentation.Web.Models.API.V1
{
    public class DataRowDTO
    {
        public int Id { get; set; }
        public int ItSystemId { get; set; }
        public string Data { get; set; }
        public int? DataTypeId { get; set; }
        public string DataTypeName { get; set; }
        public int ItInterfaceId { get; set; }
    }
}
