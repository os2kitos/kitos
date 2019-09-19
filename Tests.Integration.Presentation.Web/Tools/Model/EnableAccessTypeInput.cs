using Newtonsoft.Json;

namespace Tests.Integration.Presentation.Web.Tools.Model
{
    public class EnableAccessTypeInput
    {
        /// <summary>
        /// NOTE: Special case since this endpoint uses oData links in the path
        /// </summary>
        [JsonProperty("@odata.id")]
        public string Id { get; set; }
    }
}
