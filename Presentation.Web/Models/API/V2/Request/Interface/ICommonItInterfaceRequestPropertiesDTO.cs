namespace Presentation.Web.Models.API.V2.Request.Interface
{
    /// <summary>
    /// Properties of the write model which are shared by rightsholder and regular write models
    /// </summary>
    public interface ICommonItInterfaceRequestPropertiesDTO
    {
        public string Name { get; set; }
        public string InterfaceId { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string UrlReference { get; set; }
    }
}
